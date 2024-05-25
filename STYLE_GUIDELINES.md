# :art: Samm-Bot Style Guidelines

This document will aid you in the case that you decide to create a pull request for this repository.

> [!TIP]
> Almost everything described in this document is automatically applied by any IDE that
> supports .editorconfig files or ReSharper settings.

## :straight_ruler: Indentation Style

The indentation style that Samm-Bot uses is Allman.

You can read more about the Allman indentation style [here](https://en.wikipedia.org/wiki/Indentation_style#Allman_style).

## :gear: Module and Command Declarations

A module is defined by 4 class attributes. They may appear in this order:
* `PrettyName`
* `Group`
* `ModuleEmoji`
* `RequireOwner` (Optional)

A command may have the following attributes, in this order:
* `SlashCommand`
* `DetailedDescription`
* `RateLimit`
* `RequireOwner` (Optional)
* `RequireContext` (Optional)
* `RequireBotPermission` (Optional)
* `RequireUserPermission` (Optional)
* `RequireNsfw` (Optional)

Parameters must also have `Summary` attributes, and be placed like a class initializer. Example:
```csharp
[SlashCommand("weather", "Gets the current weather for your city.")]
[DetailedDescription("Gets the current weather forecast for your city. May not have all the information available, and the location may not be accurate.")]
[RateLimit(3, 2)]
public async Task<RuntimeResult> GetWeatherAsync
(
    [Summary("City", "The name of the city you want to get the weather forecast for.")]
    string city
)
{
    // ...
}
```

## :page_facing_up: New Lines and Spacing

You must leave 1 new line between each function/class/struct/statement.

You may group a statement (i.e. an `if` statement) group if it makes sense to do so. For example, if you are checking several cases against one variable, or it contextually makes sense to place them together.

## :id: Member Naming

Use `PascalCase` for class names and public fields, but use `camelCase` for local variables and parameters.  
Use `SCREAMING_SNAKE_CASE` for public const variables, `_SCREAMING_SNAKE_CASE` for private consts.  
Use `_PascalCase` for private properties, but `_camelCase` for private fields.

## :abc: Strings

Please use `$` (string interpolation) when placing values inside strings. `string.Format` may be used when working with externally provided strings.

Read more about string interpolation [here](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/interpolated).

## :left_speech_bubble: Comments

Comments are only needed when the action the code is performing is obscure or hard to follow. Excessive commenting is highly discouraged.  
XML documentation for functions, properties, fields and classes are encouraged.

## :card_index_dividers: Types

Please use the keyword version of types when possible, this means using `string` instead of `String` (looking at you, Java users), `int` instead of `Int32`, `uint` instead of `UInt32` and so on.
