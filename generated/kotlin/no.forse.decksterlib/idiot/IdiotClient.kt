/**
 * Autogenerated by really, really eager small hamsters.
 *
 * Notifications (events) for this game:
 * ItsTimeToSwapCards: ItsTimeToSwapCardsNotification
 * PlayerIsReady: PlayerIsReadyNotification
 * GameHasStarted: GameStartedNotification
 * GameEnded: GameEndedNotification
 * ItsYourTurn: ItsYourTurnNotification
 * PlayerDrewCards: PlayerDrewCardsNotification
 * PlayerPutCards: PlayerPutCardsNotification
 * DiscardPileFlushed: DiscardPileFlushedNotification
 * PlayerIsDone: PlayerIsDoneNotification
 * PlayerSwappedCards: PlayerSwappedCardsNotification
 * PlayerAttemptedPuttingCard: PlayerAttemptedPuttingCardNotification
 * PlayerPulledInDiscardPile: PlayerPulledInDiscardPileNotification
 *
*/
package no.forse.decksterlib.idiot

interface IdiotClient {
    suspend fun iamReady(request: IamReadyRequest): EmptyResponse
    suspend fun swapCards(request: SwapCardsRequest): SwapCardsResponse
    suspend fun putCardsFromHand(request: PutCardsFromHandRequest): DrawCardsResponse
    suspend fun putCardsFacingUp(request: PutCardsFacingUpRequest): EmptyResponse
    suspend fun putCardFacingDown(request: PutCardFacingDownRequest): PutBlindCardResponse
    suspend fun putChanceCard(request: PutChanceCardRequest): PutBlindCardResponse
    suspend fun pullInDiscardPile(request: PullInDiscardPileRequest): PullInResponse
}
