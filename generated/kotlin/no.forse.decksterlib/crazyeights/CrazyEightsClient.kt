package no.forse.decksterlib.crazyeights

interface CrazyEightsClient {
  suspend fun putCard(card: Card, cancellationToken: CancellationToken): PlayerViewOfGame
  suspend fun putEight(card: Card, newSuit: Suit, cancellationToken: CancellationToken): PlayerViewOfGame
  suspend fun drawCard(cancellationToken: CancellationToken): Card
  suspend fun pass(cancellationToken: CancellationToken): EmptyResponse
}
