# Samm-Bot Style Guidelines

This document will aid you in the case that you decide to create a pull request for this repository.

> [!TIP]
> Almost everything described in this document is automatically applied by any IDE that
> supports .editorconfig files or ReSharper settings.

## Indentation Style

The indentation style that Samm-Bot uses is Allman.

You can read more about the Allman indentation style [here](https://en.wikipedia.org/wiki/Indentation_style#Allman_style).

## Module/Command Declaration

A module is defined by 4 class attributes. They may appear in this order:
* `PrettyName`
* `Group`
* `ModuleEmoji`
* `RequireOwner` (Optional)

A command is defined by 2 or more function attributes. They may appear in this order:
* `SlashCommand`
* `DetailedDescription`
* `RateLimit`
* `RequireOwner` (Optional)
* `RequireContext` (Optional)
* `RequireBotPermission` (Optional)
* `RequireUserPermission` (Optional)
* `RequireNsfw` (Optional)

## New Lines/Spacing

You must leave 1 new line between each function/class/struct/statement.

You may group a statement (i.e. an `if` statement) group if it makes sense to do so. For example, if you are checking several cases against one variable, or it contextually makes sense to place them together.

## Variable/Class Naming

Use `PascalCase` for class names and public fields, but use `camelCase` for local variables and parameters.  
Use `SCREAMING_SNAKE_CASE` for public const variables, `_SCREAMING_SNAKE_CASE` for private consts.  
Use `_PascalCase` for private properties, but `_camelCase` for private fields.

## Strings

Please use `$` (string interpolation) when placing values inside strings. `string.Format` may be used when working with externally provided strings.

Read more about string interpolation [here](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/interpolated).

## Comments

Comments are only needed when the action the code is performing is obscure or hard to follow. Excessive commenting is highly discouraged.  
XML documentation for functions, properties, fields and classes are encouraged.

## Types

Please use the keyword version of types when possible, this means using `string` instead of `String` (looking at you, Java users), `int` instead of `Int32`, `uint` instead of `UInt32` and so on.
