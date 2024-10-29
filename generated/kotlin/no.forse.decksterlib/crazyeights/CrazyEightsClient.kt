package no.forse.decksterlib.crazyeights

interface CrazyEightsClient {
    suspend fun putCard(request: PutCardRequest): PlayerViewOfGame
    suspend fun putEight(request: PutEightRequest): PlayerViewOfGame
    suspend fun drawCard(request: DrawCardRequest): CardResponse
    suspend fun pass(request: PassRequest): EmptyResponse
}
