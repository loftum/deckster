/**
 * Autogenerated by really, really eager small hamsters.
 *
 * Notifications (events) for this game:
 * GameEnded: GameEndedNotification
 * ItsYourTurn: ItsYourTurnNotification
 * PlayerDrewCards: PlayerDrewCardsNotification
 * PlayerPutCards: PlayerPutCardsNotification
 * DiscardPileFlushed: DiscardPileFlushedNotification
 * PlayerIsDone: PlayerIsDoneNotification
 * PlayerAttemptedPuttingCard: PlayerAttemptedPuttingCardNotification
 * PlayerPulledInDiscardPile: PlayerPulledInDiscardPileNotification
 *
*/
package no.forse.decksterlib.idiot

interface IdiotClient {
    suspend fun putCardsFromHand(request: PutCardsFromHandRequest): EmptyResponse
    suspend fun putCardsFacingUp(request: PutCardsFacingUpRequest): EmptyResponse
    suspend fun putCardFacingDown(request: PutCardFacingDownRequest): PutBlindCardResponse
    suspend fun putChanceCard(request: PutChanceCardRequest): PutBlindCardResponse
    suspend fun pullInDiscardPile(request: PullInDiscardPileRequest): PullInResponse
    suspend fun drawCards(request: DrawCardsRequest): DrawCardsResponse
}
