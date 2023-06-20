# Pokedex Randomizer

This software was created to play a custom Pokemon game mode on [Pokemon Showdown](https://play.pokemonshowdown.com/), where players attempt to guess randomly-generated Pokemon using only their Pokedex entries to build a team and do battle with that team.

![Pokedex Randomizer Main Tab](https://github.com/DatPags/PokedexRandomizer/blob/master/Images/main.png?raw=true)

You can see this game in action on The Pags Crew YouTube channel:

[![Pags Crew Pokedex Battles Episode 1](https://img.youtube.com/vi/1ukPsxJvK9A/0.jpg)](https://www.youtube.com/watch?v=1ukPsxJvK9A&list=PLM6Gpbz92cYS80ohVCxBdSVTjvTKRJUrx&index=1)

## What It Does

When you first load up the software, it'll download Pokemon data and images. Any subsequent time new Pokemon are added, the software will automatically prompt you if you want to update it.

The main screen gives you a randomize button and the number of Pokemon you want to generate. This will pull up a random Pokemon and a random Pokedex entry from any game it is in, as well as some other information.

Other tabs allow you to generate a random moveset for any Pokemon, look up Pokemon info manually, or see a list of all Pokemon.

![Pokedex Randomizer Moves Tab](https://github.com/DatPags/PokedexRandomizer/blob/master/Images/moves.png?raw=true)

![Pokedex Randomizer Manual Tab](https://github.com/DatPags/PokedexRandomizer/blob/master/Images/manual.png?raw=true)

![Pokedex Randomizer Pokedex Tab](https://github.com/DatPags/PokedexRandomizer/blob/master/Images/dex.png?raw=true)

## How to Get It

Check the [releases page](https://github.com/DatPags/PokedexRandomizer/releases) to get the most recent installer. Only Windows is supported for now.

## Game Modes

We've come up with a few different ways to play this game, but more are easily possible!

In each mode, you keep going until each player has a full team of 6, then use the Moves tab to generate a random moveset for each. Whether or not you go with the randomly-generated ability is up to you. Held items and stats are manually chosen after moves are generated.

1. **Standard**
  
   Generate 1 Pokemon at a time and read only the dex entry to the other player. The other player gets one guess at each Pokemon. Each player can skip up to 3 Pokemon.

2. **Classifications**

   Generate 3 Pokemon at a time and read only the classifications of each to the other player. The other player gets to choose which of those 3 to hear the dex entry of, and gets a guess for one of those 3 Pokemon. There are no skips.

3. **Types**

   Generate 6 Pokemon at a time and read only their types to the other player. The other player can choose to hear the classifications of 2 of those 6 Pokemon. There are no skips.

## Credits

* External packages and libraries
  * HTML Agility Pack (https://html-agility-pack.net/)
  * Newtonsoft JSON (https://www.newtonsoft.com/json)
  * ImageSharp (https://github.com/SixLabors/ImageSharp)
  * Web API Client (https://dotnet.microsoft.com/en-us/apps/aspnet/apis)
* Pokemon info sourced from:
  * Pokemon Database (https://pokemondb.net/pokedex/national)
* Images sourced from
  * PokeSprite (https://github.com/msikma/pokesprite)
  * Pokemon HOME (https://home.pokemon.com/en-us/)
  * Project Pokemon (https://projectpokemon.org/home/docs/spriteindex_148/switch-sv-style-sprites-for-home-r153/)
