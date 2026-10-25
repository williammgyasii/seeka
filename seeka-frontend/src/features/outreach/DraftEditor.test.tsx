import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { DraftEditor } from './DraftEditor'

describe('DraftEditor', () => {
  it('lets the user edit and explicitly approve a draft', () => {
    const onSave = vi.fn()
    render(
      <DraftEditor
        draft={{
          id: 'draft-1',
          channel: 'email',
          subject: 'Original subject',
          body: 'Original body',
          isApproved: false,
        }}
        onSave={onSave}
        onRecordCopy={vi.fn()}
      />,
    )

    fireEvent.change(screen.getByLabelText(/draft body/i), {
      target: { value: 'Edited truthful body' },
    })
    fireEvent.click(screen.getByRole('button', { name: /approve draft/i }))

    expect(onSave).toHaveBeenCalledWith({
      subject: 'Original subject',
      body: 'Edited truthful body',
      isApproved: true,
    })
  })
})
