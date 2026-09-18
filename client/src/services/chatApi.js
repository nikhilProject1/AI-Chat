const API_BASE_URL = import.meta.env.VITE_API_URL ?? ''

async function apiRequest(path, options = {}) {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    headers: {
      'Content-Type': 'application/json',
      ...options.headers,
    },
    ...options,
  })

  if (!response.ok) {
    let message = 'Something went wrong. Please try again.'
    try {
      const error = await response.json()
      message = error.message ?? message
    } catch {
      // Keep the safe fallback message when the response is not JSON.
    }
    throw new Error(message)
  }

  return response.json()
}

export function getConversations() {
  return apiRequest('/api/conversations')
}

export function getConversation(id) {
  return apiRequest(`/api/conversations/${id}`)
}

export function sendMessage(conversationId, message) {
  return apiRequest('/api/chat', {
    method: 'POST',
    body: JSON.stringify({ conversationId, message }),
  })
}
