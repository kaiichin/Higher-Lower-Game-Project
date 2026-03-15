# Higher Lower TCP Game

this is a number guessing game made for CMGT Networking Course at Saxion, the server handles the game logic over TCP, and Unity Client is the UI side that will be connected to the server

how it works is the server will pick random number between 0 and 100. how it works is you start the server then you connect it using Unity client(just start running the unity) then you can start guessing. After guessing the number the server will tells if the answer is higher or lower, it will also track of your attempts and resets when you get the correct number

how to run ?

- start by running the server first (important)
- open client folder in unity, load the scene press play
- try to guess a number and submit

important information is that currently both are running in the default address 127.0.0.1 

features that i add 

- some color feedback for hints
- attempt counter
- guess history log
- ENTER key shortcut

## Server template and base TCP code provided by Paul Bonsma.