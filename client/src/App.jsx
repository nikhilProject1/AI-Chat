import { useEffect, useRef, useState } from 'react'
import './App.css'
import { getConversation, getConversations, sendMessage } from './services/chatApi'

function App() {
  const [conversations, setConversations] = useState([])
  const [conversationId, setConversationId] = useState(null)
  const [messages, setMessages] = useState([])
  const [input, setInput] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const messageEndRef = useRef(null)

  useEffect(() => {
    async function initializeChat() {
      try {
        const items = await getConversations()
        setConversations(items)

        if (items.length > 0) {
          const conversation = await getConversation(items[0].id)
          setConversationId(conversation.id)
          setMessages(conversation.messages)
        }
      } catch (requestError) {
        setError(requestError.message)
      }
    }

    initializeChat()
  }, [])

  useEffect(() => {
    messageEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages, loading])

  async function loadConversationList() {
    try {
      const items = await getConversations()
      setConversations(items)
    } catch (requestError) {
      setError(requestError.message)
    }
  }

  async function selectConversation(id) {
    if (loading) return
    setError('')
    try {
      const conversation = await getConversation(id)
      setConversationId(conversation.id)
      setMessages(conversation.messages)
    } catch (requestError) {
      setError(requestError.message)
    }
  }

  function startNewChat() {
    if (loading) return
    setConversationId(null)
    setMessages([])
    setInput('')
    setError('')
  }

  async function handleSubmit(event) {
    event.preventDefault()
    const content = input.trim()
    if (!content || loading) return

    const pendingMessage = {
      id: `pending-${Date.now()}`,
      role: 'user',
      content,
      createdAt: new Date().toISOString(),
    }

    setMessages((current) => [...current, pendingMessage])
    setInput('')
    setError('')
    setLoading(true)

    try {
      const response = await sendMessage(conversationId, content)
      setConversationId(response.conversationId)
      setMessages((current) => [...current, response.message])
      await loadConversationList()
    } catch (requestError) {
      setMessages((current) => current.filter((message) => message.id !== pendingMessage.id))
      setInput(content)
      setError(requestError.message)
    } finally {
      setLoading(false)
    }
  }

  function handleKeyDown(event) {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault()
      event.currentTarget.form?.requestSubmit()
    }
  }

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand">
          <span className="brand-mark">A</span>
          <span>AI Chat</span>
        </div>

        <button className="new-chat" type="button" onClick={startNewChat} disabled={loading}>
          <span aria-hidden="true">＋</span> New chat
        </button>

        <nav className="conversation-list" aria-label="Saved conversations">
          <p className="section-label">Recent conversations</p>
          {conversations.length === 0 ? (
            <p className="empty-list">Your saved chats will appear here.</p>
          ) : (
            conversations.map((conversation) => (
              <button
                className={`conversation-item ${conversation.id === conversationId ? 'active' : ''}`}
                key={conversation.id}
                type="button"
                onClick={() => selectConversation(conversation.id)}
                disabled={loading}
              >
                {conversation.title}
              </button>
            ))
          )}
        </nav>

        <p className="sidebar-note">Powered by OpenRouter</p>
      </aside>

      <main className="chat-panel">
        <header className="chat-header">
          <div>
            <p className="eyebrow">LEARNING PROJECT</p>
            <h1>{conversationId ? 'Conversation' : 'New conversation'}</h1>
          </div>
          <span className="phase-badge">Phase 1</span>
        </header>

        <section className={`messages ${messages.length === 0 ? 'empty' : ''}`} aria-live="polite">
          {messages.length === 0 ? (
            <div className="welcome">
              <div className="welcome-icon">✦</div>
              <h2>What can I help you learn?</h2>
              <p>Ask a question to start a conversation. Your chat will be saved automatically.</p>
            </div>
          ) : (
            messages.map((message) => (
              <article className={`message ${message.role}`} key={message.id}>
                <div className="avatar">{message.role === 'user' ? 'You' : 'AI'}</div>
                <div>
                  <p className="message-role">{message.role === 'user' ? 'You' : 'Assistant'}</p>
                  <p className="message-content">{message.content}</p>
                </div>
              </article>
            ))
          )}

          {loading && (
            <div className="typing" aria-label="Assistant is responding">
              <span></span><span></span><span></span>
            </div>
          )}
          <div ref={messageEndRef} />
        </section>

        <div className="composer-area">
          {error && <div className="error-message" role="alert">{error}</div>}
          <form className="composer" onSubmit={handleSubmit}>
            <textarea
              aria-label="Message"
              placeholder="Ask something..."
              rows="1"
              value={input}
              onChange={(event) => setInput(event.target.value)}
              onKeyDown={handleKeyDown}
              disabled={loading}
              maxLength={8000}
            />
            <button type="submit" disabled={loading || !input.trim()}>
              {loading ? 'Sending' : 'Send'}
            </button>
          </form>
          <p className="composer-hint">Press Enter to send · Shift + Enter for a new line</p>
        </div>
      </main>
    </div>
  )
}

export default App
