package no.forse.decksterlib.uno

interface UnoClient {
  suspend fun putCard(card: UnoCard, cancellationToken: CancellationToken): PlayerViewOfGame
  suspend fun putWild(card: UnoCard, newColor: UnoColor, cancellationToken: CancellationToken): PlayerViewOfGame
  suspend fun drawCard(cancellationToken: CancellationToken): UnoCard
  suspend fun pass(cancellationToken: CancellationToken): EmptyResponse
}
