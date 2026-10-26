import { zodResolver } from '@hookform/resolvers/zod'
import { isValidPhoneNumber, parsePhoneNumber, type CountryCode } from 'libphonenumber-js'
import { Controller, useForm, useWatch } from 'react-hook-form'
import { useSearchParams } from 'react-router-dom'
import { z } from 'zod'
import { countryCodeForTimeZone, deviceTimeZone } from '@/shared/lib/countries'
import { Brand, Button, Card, CardContent, ErrorMessage, Field, Input } from '@/shared/ui'
import { useRegisterMutation, useSignInMutation } from './authApi'
import { isPhoneCountry } from './phoneCountries'
import { PhoneNumberField } from './PhoneNumberField'

const authSchema = z
  .object({
    mode: z.enum(['login', 'register']),
    displayName: z.string(),
    email: z.email(),
    password: z.string().min(10),
    phoneCountry: z.string(),
    phoneNumber: z.string(),
  })
  .superRefine((values, context) => {
    if (values.mode === 'login') return
    if (!values.displayName.trim())
      context.addIssue({ code: 'custom', path: ['displayName'], message: 'Enter your full name.' })
    if (!isValidPhoneNumber(values.phoneNumber, values.phoneCountry as CountryCode))
      context.addIssue({
        code: 'custom',
        path: ['phoneNumber'],
        message: 'Enter a valid phone number for the chosen country code.',
      })
  })

type AuthValues = z.infer<typeof authSchema>

export function AuthPage({ timeZone = deviceTimeZone() }: { timeZone?: string }) {
  const suggestedCountry = countryCodeForTimeZone(timeZone)
  const [searchParams] = useSearchParams()
  const form = useForm<AuthValues>({
    resolver: zodResolver(authSchema),
    defaultValues: {
      mode: searchParams.get('mode') === 'register' ? 'register' : 'login',
      displayName: '',
      email: '',
      password: '',
      phoneCountry: isPhoneCountry(suggestedCountry) ? suggestedCountry : '',
      phoneNumber: '',
    },
  })
  const mode = useWatch({ control: form.control, name: 'mode' })
  const setMode = (next: AuthValues['mode']) => {
    form.clearErrors()
    form.setValue('mode', next)
  }
  const [signIn, signInState] = useSignInMutation()
  const [register, registerState] = useRegisterMutation()
  const mutation = mode === 'login' ? signInState : registerState
  const authenticate = async (values: AuthValues) => {
    // No navigation here: both mutations cache the session, and the route guards
    // move the visitor on (back to where they were, or to onboarding).
    const { email, password } = values
    if (values.mode === 'login') await signIn({ email, password })
    else
      await register({
        email,
        password,
        displayName: values.displayName.trim(),
        phoneNumber: parsePhoneNumber(values.phoneNumber, values.phoneCountry as CountryCode)
          .number,
      })
  }

  return (
    <main className="grid min-h-screen bg-background lg:grid-cols-[minmax(0,.8fr)_minmax(0,1.2fr)]">
      <section className="relative isolate overflow-hidden bg-primary px-6 pt-7 pb-10 text-primary-foreground sm:px-10 lg:flex lg:flex-col lg:justify-between lg:p-12">
        <div aria-hidden className="pointer-events-none absolute inset-0 -z-10">
          <div className="absolute -top-32 -left-24 size-96 animate-glow-drift rounded-full bg-highlight/25 blur-3xl motion-reduce:animate-none" />
          <div className="absolute -right-40 -bottom-40 size-[30rem] animate-glow-drift rounded-full bg-primary-foreground/15 blur-3xl [animation-delay:-9s] motion-reduce:animate-none" />
          <div className="absolute inset-0 bg-grain opacity-10 mix-blend-overlay" />
        </div>
        <Brand onPrimary />
        <div className="mt-10 max-w-md animate-in duration-700 fade-in slide-in-from-bottom-4 motion-reduce:animate-none lg:mt-0">
          <p className="mb-4 flex items-center gap-2 text-xs font-semibold tracking-[.25em] uppercase opacity-80 sm:text-sm">
            <span className="size-2 rounded-full bg-highlight" />
            Remote work, fine print checked
          </p>
          <h1 className="font-heading text-4xl leading-[1.05] font-semibold tracking-tight sm:text-5xl lg:text-[2.75rem] xl:text-5xl">
            Find work that can <span className="text-highlight">actually hire you.</span>
          </h1>
          <p className="mt-5 hidden max-w-md text-base leading-7 opacity-80 sm:block">
            Seeka checks where a role can hire before it scores your fit, then helps you
            reach the right people with outreach you approve.
          </p>
          <ul className="mt-7 hidden flex-wrap gap-2 sm:flex">
            {['Eligibility first', 'Honest fit scores', 'Outreach you approve'].map((point) => (
              <li
                key={point}
                className="rounded-full border border-primary-foreground/15 bg-primary-foreground/10 px-3 py-1 text-sm backdrop-blur"
              >
                {point}
              </li>
            ))}
          </ul>
        </div>
        <p className="hidden text-sm opacity-60 lg:block">
          Built for remote professionals, wherever they live.
        </p>
      </section>
      <section className="flex items-start justify-center px-5 py-8 sm:py-14 lg:items-center">
        <Card className="w-full max-w-md animate-in px-3 py-7 delay-150 duration-700 fill-mode-both fade-in slide-in-from-bottom-4 motion-reduce:animate-none sm:px-5 sm:py-9">
          <CardContent>
            <p className="text-sm font-medium text-muted-foreground">
              {mode === 'login' ? 'Welcome back' : 'Create your workspace'}
            </p>
            <h2 className="mt-2 font-heading text-3xl font-semibold tracking-tight">
              {mode === 'login' ? 'Sign in to Seeka' : 'Start your job search'}
            </h2>
            <form className="mt-7 grid gap-4" onSubmit={form.handleSubmit(authenticate)}>
              {mode === 'register' && (
                <Field label="Full name" error={form.formState.errors.displayName?.message}>
                  <Input autoComplete="name" {...form.register('displayName')} />
                </Field>
              )}
              <Field label="Email address" error={form.formState.errors.email?.message}>
                <Input type="email" autoComplete="email" {...form.register('email')} />
              </Field>
              <Field label="Password" error={form.formState.errors.password?.message}>
                <Input
                  type="password"
                  autoComplete={mode === 'login' ? 'current-password' : 'new-password'}
                  {...form.register('password')}
                />
              </Field>
              {mode === 'register' && (
                <Controller
                  control={form.control}
                  name="phoneCountry"
                  render={({ field }) => (
                    <PhoneNumberField
                      country={field.value}
                      onCountryChange={field.onChange}
                      inputProps={form.register('phoneNumber')}
                      error={form.formState.errors.phoneNumber?.message}
                    />
                  )}
                />
              )}
              {mutation.error && <ErrorMessage error={mutation.error} />}
              <Button size="lg" className="mt-2 w-full" disabled={mutation.isLoading}>
                {mutation.isLoading
                  ? 'Please wait…'
                  : mode === 'login'
                    ? 'Sign in'
                    : 'Create account'}
              </Button>
            </form>
            <Button
              variant="link"
              className="mt-6 w-full text-muted-foreground"
              onClick={() => setMode(mode === 'login' ? 'register' : 'login')}
            >
              {mode === 'login'
                ? 'New to Seeka? Create an account'
                : 'Already have an account? Sign in'}
            </Button>
          </CardContent>
        </Card>
      </section>
    </main>
  )
}
