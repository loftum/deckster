package no.forse.decksterlib.chatroom

interface ChatClient {
    suspend fun chatAsync(event: SendChatRequest): ChatResponse
}
