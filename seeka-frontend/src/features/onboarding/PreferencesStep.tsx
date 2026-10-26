import { zodResolver } from '@hookform/resolvers/zod'
import { Controller, useForm, useWatch } from 'react-hook-form'
import { z } from 'zod'
import {
  employmentTypes,
  type CandidateProfile,
  type JobPreferences,
  type PayBand,
  type PayBandOptions,
  type ResumeDraft,
} from '@/shared/api'
import { ErrorMessage, Loading } from '@/shared/ui'
import { ChoiceField } from './ChoiceField'
import { errorAt } from './formValues'
import { useCompleteOnboardingMutation, useGetPayBandsQuery } from './onboardingApi'
import { residenceCode } from './residence'
import { StepFrame } from './StepFrame'
import { TitlePicker } from './TitlePicker'
import { useStepSave } from './useStepSave'

const fallbackCurrency = 'USD'
const currencyNames = new Intl.DisplayNames(['en'], { type: 'currency' })
const currencyLabel = (code: string) => `${code} – ${currencyNames.of(code) ?? code}`
const amountFormat = new Intl.NumberFormat('en')

function bandLabel(currency: string, band: PayBand) {
  const at = (value: number) => `${currency} ${amountFormat.format(value)}`
  if (band.maximum === null) return `${at(band.minimum)}+`
  if (band.minimum === 0) return `Under ${at(band.maximum)}`
  return `${at(band.minimum)} – ${amountFormat.format(band.maximum)}`
}

const schema = z.object({
  targetJobTitles: z.array(z.string()).min(1, 'Add at least one role you are looking for.'),
  employmentTypes: z.array(z.string()),
  compensationCurrency: z.string(),
  // The chosen band's position in the currency's list, or '' for no pay.
  payBand: z.string(),
  // Holds server errors for the band; the saved amounts are built from payBand.
  minimumCompensation: z.string(),
})

type Values = z.infer<typeof schema>

function initialValues(
  profile: CandidateProfile,
  draft: ResumeDraft | null,
  options: PayBandOptions | undefined,
): Values {
  const saved = profile.preferences
  const byCurrency = options?.byCurrency ?? {}
  const country = residenceCode(profile.about.countryOfResidence)
  const currency =
    saved?.compensationCurrency && byCurrency[saved.compensationCurrency]
      ? saved.compensationCurrency
      : ((country ? options?.byCountry[country] : undefined) ?? fallbackCurrency)
  const band = (byCurrency[currency] ?? []).findIndex(
    (item) =>
      item.minimum === saved?.minimumCompensation && item.maximum === saved?.maximumCompensation,
  )
  return {
    targetJobTitles: saved?.targetJobTitles ?? draft?.targetJobTitles ?? [],
    employmentTypes: saved?.employmentTypes ?? [],
    compensationCurrency: currency,
    payBand: band < 0 ? '' : String(band),
    minimumCompensation: '',
  }
}

function toBody(values: Values, options: PayBandOptions | undefined): JobPreferences {
  const band = values.payBand
    ? options?.byCurrency[values.compensationCurrency]?.[Number(values.payBand)]
    : undefined
  return {
    targetJobTitles: values.targetJobTitles,
    employmentTypes: employmentTypes.filter((type) => values.employmentTypes.includes(type)),
    minimumCompensation: band?.minimum ?? null,
    maximumCompensation: band?.maximum ?? null,
    compensationCurrency: band ? values.compensationCurrency : null,
  }
}

type StepProps = {
  profile: CandidateProfile
  draft: ResumeDraft | null
  onBack: () => void
  onFinished: () => void
}

export function PreferencesStep(props: StepProps) {
  const payBands = useGetPayBandsQuery()
  if (payBands.isLoading) return <Loading />
  return <PreferencesForm {...props} payBands={payBands.data} />
}

function PreferencesForm({
  profile,
  draft,
  onBack,
  onFinished,
  payBands,
}: StepProps & { payBands: PayBandOptions | undefined }) {
  const form = useForm<Values>({
    resolver: zodResolver(schema),
    defaultValues: initialValues(profile, draft, payBands),
  })
  const currency = useWatch({ control: form.control, name: 'compensationCurrency' })
  const bands = payBands?.byCurrency[currency] ?? []
  const { submit, saving, failure } = useStepSave('preferences', form.setError)
  const [complete, completion] = useCompleteOnboardingMutation()

  // Finishing is two calls: save this step, then ask the server to close
  // onboarding. The server refuses if a required step is still missing.
  const finish = form.handleSubmit(async (values) => {
    if (!(await submit(toBody(values, payBands)))) return
    const result = await complete()
    if (!result.error) onFinished()
  })

  return (
    <StepFrame
      title="Job preferences"
      description="What you are looking for. Seeka uses these as fit inputs, not hard promises."
      onSubmit={finish}
      onBack={onBack}
      saving={saving || completion.isLoading}
      failure={failure}
      submitLabel="Finish"
    >
      <Controller
        control={form.control}
        name="targetJobTitles"
        render={({ field, fieldState }) => (
          <TitlePicker
            label="Target job titles"
            value={field.value}
            onChange={field.onChange}
            error={fieldState.error?.message}
          />
        )}
      />
      <fieldset className="grid gap-2">
        <legend className="mb-2 text-sm font-medium">Employment types</legend>
        <div className="flex flex-wrap gap-x-5 gap-y-2">
          {employmentTypes.map((type) => (
            <label key={type} className="flex items-center gap-2 text-sm">
              <input
                type="checkbox"
                value={type}
                className="size-4 accent-primary"
                {...form.register('employmentTypes')}
              />
              {type}
            </label>
          ))}
        </div>
      </fieldset>
      <fieldset className="grid gap-2">
        <legend className="mb-2 text-sm font-medium">Pay (optional)</legend>
        <div className="grid gap-4 sm:grid-cols-2">
          <ChoiceField
            control={form.control}
            name="compensationCurrency"
            label="Currency"
            placeholder="Choose a currency"
            options={Object.keys(payBands?.byCurrency ?? {})}
            optionLabel={currencyLabel}
            onValueChange={() => form.setValue('payBand', '')}
          />
          <ChoiceField
            control={form.control}
            name="payBand"
            label="Monthly pay"
            placeholder="Not set"
            options={bands.map((_, index) => String(index))}
            optionLabel={(index) => bandLabel(currency, bands[Number(index)])}
          />
        </div>
        {errorAt(form, 'minimumCompensation') && (
          <span className="text-xs text-destructive">{errorAt(form, 'minimumCompensation')}</span>
        )}
      </fieldset>
      {completion.error ? <ErrorMessage error={completion.error} /> : null}
    </StepFrame>
  )
}
