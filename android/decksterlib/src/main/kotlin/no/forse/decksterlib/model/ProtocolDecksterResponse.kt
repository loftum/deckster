/**
 *
 * Please note:
 * This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * Do not edit this file manually.
 *
 */

@file:Suppress(
    "ArrayInDataClass",
    "EnumEntryName",
    "RemoveRedundantQualifierName",
    "UnusedImport"
)

package no.forse.decksterlib.model

import com.fasterxml.jackson.annotation.JsonSubTypes
import com.fasterxml.jackson.annotation.JsonTypeInfo

/**
 * 
 *
 * @param type 
 */
@JsonTypeInfo(use = JsonTypeInfo.Id.NAME, include = JsonTypeInfo.As.PROPERTY, property = "type", visible = true)
@JsonSubTypes(
    JsonSubTypes.Type(value = ChatRoomChatResponse::class, name = "ChatRoom.ChatResponse"),
    JsonSubTypes.Type(value = CommonFailureResponse::class, name = "Common.FailureResponse"),
    JsonSubTypes.Type(value = CrazyEightsCardResponse::class, name = "CrazyEights.CardResponse"),
    JsonSubTypes.Type(value = CrazyEightsCrazyEightsFailureResponse::class, name = "CrazyEights.CrazyEightsFailureResponse"),
    JsonSubTypes.Type(value = CrazyEightsCrazyEightsResponse::class, name = "CrazyEights.CrazyEightsResponse"),
    JsonSubTypes.Type(value = CrazyEightsCrazyEightsSuccessResponse::class, name = "CrazyEights.CrazyEightsSuccessResponse"),
    JsonSubTypes.Type(value = CrazyEightsPlayerViewOfGame::class, name = "CrazyEights.PlayerViewOfGame"),
    JsonSubTypes.Type(value = UnoPlayerViewOfUnoGame::class, name = "Uno.PlayerViewOfUnoGame"),
    JsonSubTypes.Type(value = UnoUnoCardResponse::class, name = "Uno.UnoCardResponse"),
    JsonSubTypes.Type(value = UnoUnoCardsResponse::class, name = "Uno.UnoCardsResponse"),
    JsonSubTypes.Type(value = UnoUnoFailureResponse::class, name = "Uno.UnoFailureResponse"),
    JsonSubTypes.Type(value = UnoUnoResponse::class, name = "Uno.UnoResponse"),
    JsonSubTypes.Type(value = UnoUnoSuccessResponse::class, name = "Uno.UnoSuccessResponse")
)

interface ProtocolDecksterResponse : ProtocolDecksterMessage {


}
