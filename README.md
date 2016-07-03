Aurora
==============================

A basic client for [Aura](https://github.com/aura-project/aura),
developed for fun and experimentation, and as an example for the community.
It should *not* be considered a replacement, at least not yet.

Status
------------------------------
Aurora currently supports only *very* basic features and has a long way to go.
It's also limited to the Tir region, as no other regions have been added yet.

One major problem are the props. While Aura stops you from running through
walls/props, you don't see them on the client side. Maybe Aura's regioninfo
could be used to load and place the props at the same locations, but I had
problems getting the file to load in Unity.

### Working "features"
- Character selection
- Logging in (Tir only)
- Moving
- Chatting/Whispering

Compatibility
------------------------------
Since Aura has to be kept up-to-date for compatibility with the NA client,
Aurora will be updated alongside it and will only ever support the latest
version of Aura.

**Note (2016-07-03)**<br/>
Aurora hasn't been updated in a while and might currently
not be compatible with Aura.

Requirements/Installation
------------------------------
To compile Aurora, you need the latest version of [Unity](http://unity3d.com/).
Download this repository, load it into Unity, and build it. Done!

By default, the client tries to connect to the login server at
`127.0.0.1:11000`, if you want it to connect to somewhere else,
use the `logip` and `logport` arguments, just like the official client.

Contribution
------------------------------
Feel free to submit pull requests if you'd like to help extending
this project. Maybe in time we'll get an actual alternative client.

Links
------------------------------
* Forums: http://aura-project.org/
* GitHub: https://github.com/aura-project
