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
 * @param type 
 * @param card 
 */


data class UnoUnoCardResponse (

    @get:JsonProperty("type")
    override val type: kotlin.String? = null,

    @get:JsonProperty("card")
    val card: UnoUnoCardResponseAllOfCard? = null

) : UnoUnoResponse {


}

