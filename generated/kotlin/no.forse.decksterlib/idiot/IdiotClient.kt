package no.forse.decksterlib.idiot

interface IdiotClient {
    suspend fun putCardsFromHand(request: PutCardsFromHandRequest): EmptyResponse
    suspend fun putFaceUpTableCard(request: PutFaceUpTableCardsRequest): EmptyResponse
    suspend fun putFaceDownTableCard(request: PutFaceDownTableCardRequest): EmptyResponse
    suspend fun drawCards(request: DrawCardsRequest): DrawCardsResponse
}
