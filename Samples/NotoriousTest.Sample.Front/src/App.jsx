import { useState } from 'react'

export default function App() {
  const [status, setStatus] = useState(null)

  async function createUser() {
    setStatus('pending')
    try {
      const response = await fetch('/api/users', { method: 'POST' })
      setStatus(response.ok ? 'created' : 'error')
    } catch {
      setStatus('error')
    }
  }

  return (
    <main className="app">
      <button type="button" onClick={createUser} disabled={status === 'pending'}>
        CreateUser
      </button>
      {status === 'created' && <p role="status">User created</p>}
      {status === 'error' && <p role="alert">User creation failed</p>}
    </main>
  )
}
