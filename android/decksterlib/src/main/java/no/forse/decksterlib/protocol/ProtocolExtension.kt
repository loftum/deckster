package no.forse.decksterlib.protocol

import no.forse.decksterlib.model.protocol.DecksterMessage
import kotlin.reflect.KClass

fun DecksterMessage.getType(): String = typeOf(this::class)

fun <T> typeOf(clazz: KClass<T>)  : String where T: Any {
    return clazz.qualifiedName!!.replace("no.forse.decksterlib.model.", "")
}