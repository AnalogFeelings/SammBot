# Samm-Bot Style Guidelines

This document will aid you in the (rare) case that you decide to create a pull request for this repository.

## Indentation Style

The indentation style that Samm-Bot uses is Allman with tabs. Tabs, as you may know, are 1 character, but they are equivalent to 4 spaces.

You can read more about indentation styles [here](https://en.wikipedia.org/wiki/Indentation_style#Allman_style).

## Module/Command Declaration

A module is defined by 4 class attributes. They may appear in this order:
* `Name`
* `Group`
* `Summary`
* `ModuleEmoji`

A command is defined by 2 or more function attributes. They may appear in this order:
* `Command`
* `Alias` (Optional)
* `Summary`
* `FullDescription`
* `RateLimit`
* `RequireOwner` (Optional)
* `RequireContext` (Optional)
* `RequireBotPermission` (Optional)
* `RequireUserPermission` (Optional)

## New Lines/Spacing

You must leave 1 new line between each function/class/struct/statement.

You may group a statement (i.e. an `if` statement) group if it makes sense to do so. For example, if you are checking several cases against one variable, or it contextually makes sense to place them together.

## Variable/Class Naming

Use `PascalCase` for class names, variables and parameters, but use `camelCase` for local variables.
Use `SCREAMING_SNAKE_CASE` for const variables.

## Strings

Please use `$` (string interpolation) when placing values inside strings. `string.Format` may be used when working with externally provided strings.

Read more about string interpolation [here](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/interpolated).

## Comments

Comments are only needed when the action the code is performing is obscure. Excessive commenting is highly discouraged.

## Types

Please use the keyword version of types when possible, this means using `string` instead of `String` (looking at you, Java users), `int` instead of `Int32`, `uint` instead of `UInt32` and so on.

Do the exact opposite when working with Binary files, where knowing the datatype in detail is crucial.
