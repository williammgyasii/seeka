import type { CandidateProfile, Resume } from '@/shared/api'

export const emptyProfile: CandidateProfile = {
  about: {
    fullName: '',
    headline: null,
    countryOfResidence: '',
    timeZone: '',
    seniority: null,
    yearsOfExperience: 0,
    experienceSummary: null,
    skills: [],
    languages: [],
    links: [],
  },
  experience: [],
  education: [],
  certifications: [],
  projects: [],
  volunteering: [],
  preferences: null,
  completedSteps: [],
  onboardingComplete: false,
  suggestedYearsOfExperience: null,
}

export const extracted: Resume = {
  fileName: 'cv.pdf',
  uploadedAt: '2026-10-25T12:00:00Z',
  status: 'Extracted',
  draft: {
    fullName: 'Ama Mensah',
    headline: 'HR operations lead',
    seniority: 'Senior',
    yearsOfExperience: 8,
    experienceSummary: 'Eight years leading HR operations for distributed teams.',
    skills: ['Recruitment', 'Payroll'],
    targetJobTitles: ['HR Manager'],
    details: {
      experience: [
        {
          company: 'Acme Ltd',
          title: 'HR Manager',
          location: 'Accra',
          startMonth: '2021-06',
          endMonth: null,
          current: true,
          highlights: ['Ran payroll for 300 staff', 'Cut hiring time by a third'],
        },
      ],
      education: [
        {
          school: 'University of Ghana',
          degree: 'BSc',
          fieldOfStudy: 'Psychology',
          startYear: 2012,
          endYear: 2016,
        },
      ],
      certifications: [{ name: 'SHRM-CP', issuer: 'SHRM', year: 2020 }],
      languages: [{ language: 'Twi', level: 'Fluent' }],
      links: [{ label: 'LinkedIn', url: 'https://linkedin.com/in/ama' }],
      projects: [{ name: 'Onboarding revamp', description: 'Cut onboarding to a week.', url: null }],
      volunteering: [],
    },
  },
}
