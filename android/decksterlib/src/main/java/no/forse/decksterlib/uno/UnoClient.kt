package no.forse.decksterlib.uno

import kotlinx.coroutines.flow.*
import kotlinx.coroutines.launch
import no.forse.decksterlib.DecksterServer
import no.forse.decksterlib.game.GameClientBase
import no.forse.decksterlib.model.common.EmptyResponse
import no.forse.decksterlib.model.protocol.DecksterNotification
import no.forse.decksterlib.model.uno.*
import no.forse.decksterlib.protocol.typeOf
import threadpoolScope

class UnoClient(server: DecksterServer) : GameClientBase(gameName = "uno", decksterServer = server) {

    private var gameState: PlayerViewOfGame? = null

    private val _yourTurnFlow: MutableSharedFlow<PlayerViewOfGame> = MutableSharedFlow(replay = 0, extraBufferCapacity = 1)
    val yourTurnFlow: SharedFlow<PlayerViewOfGame>
        get() = _yourTurnFlow

    val notificationFlow: Flow<DecksterNotification>?
        get() = joinedGame?.notificationFlow

    override suspend fun onNotificationArrived(notif: DecksterNotification) {
        when (notif) {
            is ItsYourTurnNotification -> onItsYourTurn(notif)
            is PlayerPutCardNotification -> onPlayerPutCard(notif)
            is PlayerDrewCardNotification -> onPlayerDrewCard(notif)
            is PlayerPassedNotification -> onPlayerPassed(notif)
            is PlayerPutWildNotification -> onPlayerPutWild(notif)
            is RoundStartedNotification -> onRoundStarted(notif)
            is RoundEndedNotification -> onRoundEnded(notif)
            is GameStartedNotification -> onGameStarted(notif)
            is GameEndedNotification -> onGameEnded(notif)
        }
        println("Uno notification:\n$notif")
    }

    private fun onGameStarted(notif: GameStartedNotification) {
    }

    private fun onGameEnded(notif: GameEndedNotification) {
    }

    private fun onRoundStarted(notif: RoundStartedNotification) {
        gameState = notif.playerViewOfGame
    }

    private fun onRoundEnded(notif: RoundEndedNotification) {

    }

    private fun onItsYourTurn(notif: ItsYourTurnNotification) {
        gameState = notif.playerViewOfGame
        threadpoolScope.launch {
            _yourTurnFlow.emit(notif.playerViewOfGame)
        }
    }

    private fun onPlayerPassed(notif: PlayerPassedNotification) {
    }

    private fun onPlayerDrewCard(notif: PlayerDrewCardNotification) {

    }

    private fun onPlayerPutCard(notif: PlayerPutCardNotification) {
    }

    private fun onPlayerPutWild(notif: PlayerPutWildNotification) {
    }

    override fun onGameLeft() {
    }

    override fun onGameJoined() {

    }

    // Commands
    suspend fun putCard(card: UnoCard) : PlayerViewOfGame {
        return sendAndReceive<PlayerViewOfGame>(
            PutCardRequest(
                typeOf(PutCardRequest::class),
                joinedGameOrThrow.userUuid,
                card
            )
        )
    }

    suspend fun putWild(unoCard: UnoCard, unoColor: UnoColor) : PlayerViewOfGame {
        return sendAndReceive(
            PutWildRequest(
                typeOf(PutWildRequest::class),
                joinedGameOrThrow.userUuid,
                unoCard,
                unoColor,
            )
        )
    }

    suspend fun drawCard() : UnoCardResponse {
        return sendAndReceive(
            DrawCardRequest(
                typeOf(DrawCardRequest::class),
                joinedGameOrThrow.userUuid
            )
        )
    }

    suspend fun pass() : EmptyResponse {
        return sendAndReceive(
            PassRequest(
                typeOf(PassRequest::class),
                joinedGameOrThrow.userUuid
            ),
        )
    }
}