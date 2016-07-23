# Pokémon Go Client API

Interface to Pokémon Go Client

**Read previous issues before opening a new one! Maybe your issue is already answered. Questions will be removed.

----------
### Usage Example

```
var client = new Client(new Settings()); //Define your own ISettings implementation
await _client.DoGoogleLogin();
await _client.SetServer();
var profile = await _client.GetProfile();
var settings = await _client.GetSettings();
var mapObjects = await _client.GetMapObjects();
```

----------

### What is Pokémon Go?
According to [the company](http://www.pokemon.com/us/pokemon-video-games/pokemon-go/):

> “Travel between the real world and the virtual world of Pokémon with Pokémon GO for iPhone and Android devices. With Pokémon GO, you’ll discover Pokémon in a whole new world—your own! Pokémon GO is built on Niantic’s Real World Gaming Platform and will use real locations to encourage players to search far and wide in the real world to discover Pokémon. Pokémon GO allows you to find and catch more than a hundred species of Pokémon as you explore your surroundings.”

# License

This Project is licensed as GNU (GNU GENERAL PUBLIC LICENSE v3) 

## Legal

This code is in no way affiliated with, authorized, maintained, sponsored or endorsed by Niantic, The Pokémon Company, Nintendo or any of its affiliates or subsidiaries. This is an independent and unofficial API. Use at your own risk.
