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


import com.fasterxml.jackson.annotation.JsonProperty

/**
 * 
 *
 * @param name 
 * @param numberOfCards 
 */


data class UnoOtherUnoPlayer (

    @get:JsonProperty("name")
    val name: kotlin.String? = null,

    @get:JsonProperty("numberOfCards")
    val numberOfCards: kotlin.Int? = null

) {


}

