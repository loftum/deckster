import java.util.regex.Pattern
import kotlin.io.path.Path

plugins {
    id("java-library")
    alias(libs.plugins.jetbrains.kotlin.jvm)
    alias(libs.plugins.openapi.generator)
}

java {
    sourceCompatibility = JavaVersion.VERSION_17
    targetCompatibility = JavaVersion.VERSION_17
    sourceSets["main"].java {
        srcDir("src-gen/src/main/kotlin")
    }
}

kotlin {
    jvmToolchain(17)
}

dependencies {
    implementation(libs.okhttp)
    implementation(libs.jackson.annotations)
    implementation(libs.jackson.kotlin)
    implementation(libs.retrofit)
    implementation(libs.retrofit.converter.scalars)
    implementation(libs.retrofit.converter.jackson)
    implementation(libs.kotlinx.coroutines)
    testImplementation(libs.junit)
    implementation(libs.okhttp)
    implementation(libs.okhttp.logging)
}

tasks.register("generateDtos", org.openapitools.generator.gradle.plugin.tasks.GenerateTask::class.java) {
    val packageRoot = "no.forse.decksterlib"
    // IMPORANT: DELETE build.gradle GENERATED BY THIS TASK, OR STUFF WON'T COMPILE
    // open-api-generate docs: https://github.com/OpenAPITools/openapi-generator/blob/master/modules/openapi-generator-gradle-plugin/README.adoc
    group = "openapi"
    println ("$projectDir")
    description = "Generate DTO classes for DecksterLib"
    generatorName.set("kotlin")
    // kotlin generator docs: https://openapi-generator.tech/docs/generators/kotlin/
    // Templates: https://github.com/OpenAPITools/openapi-generator/blob/master/modules/openapi-generator/src/main/resources/kotlin-client/data_class.mustache
    verbose.set(false)
    cleanupOutput.set(true)
    templateDir.set("$projectDir/openapi-templates")
    outputDir.set("$projectDir/src-gen")
    skipValidateSpec.set(false)
    inputSpec.set("$projectDir/../../decksterapi.yml")
    ignoreFileOverride.set("$projectDir/.openapi-generator-ignore")
    packageName.set("${packageRoot}.rest")
    apiPackage.set("${packageRoot}.rest")
    modelPackage.set("${packageRoot}.model")
    library.set("jvm-retrofit2")
    configOptions = mapOf(
        "serializationLibrary" to "jackson",
        "useCoroutines" to "true"
    )
    generateModelDocumentation.set(false)
    generateApiDocumentation.set(false)
    generateApiTests.set(false)
    generateModelTests.set(false)
    openapiNormalizer.set(mapOf("REF_AS_PARENT_IN_ALLOF" to "true"))
    // modelFilesConstrainedTo.set(emptyList())
    //supportingFilesConstrainedTo.set(listOf("*.kt", "**/*.kt", "build.gradle"))
    // Normalizing should have fixed this https://github.com/OpenAPITools/openapi-generator/issues/6080
    // can't quite get it to work
    // https://github.com/OpenAPITools/openapi-generator/blob/master/docs/customization.md#openapi-normalizer

    //templateDir.set("$projectDir/openapi-templates")
    //modelNameSuffix.set("DTO")
    doNotTrackState("Files will be deleted in task FixClassPackagesAfterOpenApi")
}


abstract class FixClassPackagesAfterOpenApi : DefaultTask() {
    companion object {
        val myPackageRoot = "no.forse.decksterlib.model"
        val packageSeparator = "XXX"
        val NL = System.lineSeparator()
    }

    @TaskAction
    fun action() {

        val modelRoot = Path(project.projectDir.toString(), "src-gen", "src", "main", "kotlin" ,"no", "forse", "decksterlib", "model")
        val generatedFiles = modelRoot.toFile().listFiles()!!
        println(modelRoot)

        logger.info("Processing files...")
        for (file in generatedFiles) {
            if (!file.name.contains(packageSeparator)) continue
            val (packageNameUpr, fileName) = file.name.split(packageSeparator)
            val packageName = packageNameUpr.lowercase()
            val fullPackage = "$myPackageRoot.$packageName"
            Path(modelRoot.toString(), packageName).toFile().mkdirs()
            val destFile = Path(modelRoot.toString(), packageName, fileName).toFile()
            copyFileAndAlterPackage(file, destFile, packageNameUpr, fullPackage)
            file.delete()
            logger.info("Package: $fullPackage File: $fileName")
        }
    }

    fun copyFileAndAlterPackage(source: File, dest: File, packageNameUpr: String, fullPackage: String) {
        dest.delete()
        val regex = Pattern.compile("(\\w+)$packageSeparator").toRegex()
        val jsonSubtyperegex = Pattern.compile("(\\w+)$packageSeparator(\\w+)::class").toRegex()
        source.readLines(Charsets.UTF_8).map { line ->
            if (line.startsWith("package")) {
                "package $fullPackage"
            } else {
                line
                    .replace(jsonSubtyperegex) { res ->
                        "$myPackageRoot." + res.groupValues[1].lowercase() + "." + res.groupValues[2] + "::class" // DecksterMessage.kt
                    }
                    .replace(regex) { res ->
                        if (line.contains("import")) {
                            res.groupValues[1].lowercase() + "." // In import
                        } else if (line.contains("JsonSubTypes")) {
                            res.groupValues[1] + "." // In import
                        } else {
                           "" // in class definition, extends
                        }
                    }
                    .replace("${packageNameUpr}$packageSeparator", "")
            }
        }.forEach {
            dest.appendText(it + NL, Charsets.UTF_8)
        }
    }
}

tasks.register<FixClassPackagesAfterOpenApi>("fixClassPackagesAfterOpenApi") {
    group = "openapi"
    description = "Since stupid OpenApi does not support package name. We have to fix it. I hate having to do stuff"
    dependsOn("generateDtos")
}
