/**
 * Autogenerated by really, really eager small hamsters.
 *
 * Notifications (events) for this game:
 * PlayerSaid: ChatNotification
 *
*/
package no.forse.decksterlib.chatroom

interface ChatClient {
    suspend fun chatAsync(event: SendChatRequest): ChatResponse
}
