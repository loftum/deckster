package no.forse.decksterlib.uno

interface UnoClient {
    suspend fun putCard(request: PutCardRequest): PlayerViewOfGame
    suspend fun putWild(request: PutWildRequest): PlayerViewOfGame
    suspend fun drawCard(request: DrawCardRequest): UnoCardResponse
    suspend fun pass(request: PassRequest): EmptyResponse
}
