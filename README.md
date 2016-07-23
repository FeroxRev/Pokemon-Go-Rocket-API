# Pokemon Go API Client Library Proof of Concept#

Example:

```

var client = new Client(new Settings()); //Define your own ISettings implementation
await _client.DoGoogleLogin();
await _client.SetServer();
var profile = await _client.GetProfile();
var settings = await _client.GetSettings();
var mapObjects = await _client.GetMapObjects();
```
