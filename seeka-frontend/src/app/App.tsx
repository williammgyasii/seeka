import { Navigate, Route, Routes } from 'react-router-dom'
import { AuthPage } from '@/features/auth/AuthPage'
import { DashboardPage } from '@/features/dashboard/DashboardPage'
import { ImportJobPage } from '@/features/jobs/ImportJobPage'
import { JobDetailPage } from '@/features/jobs/JobDetailPage'
import { PipelinePage } from '@/features/jobs/PipelinePage'
import { OnboardingPage } from '@/features/onboarding/OnboardingPage'
import { OutreachPanel } from '@/features/outreach/OutreachPanel'
import { SettingsPage } from '@/features/settings/SettingsPage'
import { AppLayout } from './AppLayout'
import { OnboardingLayout } from './OnboardingLayout'
import { RedirectIfSignedIn } from './RedirectIfSignedIn'
import { RequireSession } from './RequireSession'

function App() {
  return (
    <Routes>
      <Route element={<RedirectIfSignedIn />}>
        <Route path="/login" element={<AuthPage />} />
      </Route>
      <Route element={<RequireSession />}>
        <Route element={<OnboardingLayout />}>
          <Route path="/onboarding" element={<OnboardingPage />} />
        </Route>
        <Route path="/" element={<AppLayout />}>
          <Route index element={<Navigate to="/dashboard" replace />} />
          <Route path="dashboard" element={<DashboardPage />} />
          <Route path="jobs/import" element={<ImportJobPage />} />
          <Route
            path="jobs/:id"
            element={<JobDetailPage outreach={(detail) => <OutreachPanel detail={detail} />} />}
          />
          <Route path="pipeline" element={<PipelinePage />} />
          <Route path="settings" element={<SettingsPage />} />
        </Route>
      </Route>
      <Route path="*" element={<Navigate to="/dashboard" replace />} />
    </Routes>
  )
}

export default App
