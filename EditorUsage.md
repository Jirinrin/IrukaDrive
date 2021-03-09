# Editor usage

## Get started
- Click `[New Song]`. This creates a new song for you and a beginner drive chart to go along with it.
- Go to `[Song data]`
  - Most important: `[Select Audio]` and set the BPM of the song
  - You may need to do some additional tweaking to get the beats matched up with your song: for this you can use the `[Beat Offset]` field, and the `[Listen]` button in the bottom right can help you with quickly tweaking this until it's just right
  - (And you can set more metadata of the song and chart in the Song Data and Chart Data menus)
- Place your first word with `Tab`
- See with `Spacebar` how it plays! And press `Escape` to go back


## Controls
- Navigation
  - `rightclick+drag↔` to pan
    - hold `alt` to pan quickly!
  - `shift+rightclick+drag↕` to zoom
  - `Home`/`End` to go to the start/end of your chart
- Playtesting
  - `Spacebar` to start playtest from your mouse pointer position in autoplay
  - `Shift+Spacebar` to start playtest in manual play
  - While in playtest, press `Escape` to exit
- Click somewhere in empty space to put your cursor there
  - `Tab`/`N` to make a new word (or `Alt+click`ing in empty space also works)
  - `Ctrl+V` to paste (in case)
- Click a word to select it
  - `Ctrl+C` to copy
  - `F2`/`R` to edit the word (or `Alt+click`ing it also works) => `Enter`/`Escape` to submit
  - `Del` to delete the word (or removing all letters and submitting also works)
- Dragging on a word
  - Normally drag on a word to move it
  - Drag with right mouse button to change its beat spacing
- Special stuff when editing words:
  - _Chord_: start the word with `[[`
  - _Randomly generated English word of length_: make a word consisting entirely of the letter `x`
  - A word can have spaces -- these will just count as beats where you don't have to input anything in gameplay
  - Use `Shift+Space` to type the `⎵` character -- this counts as a `Spacebar` character to input in gameplay
- `Ctrl+Z`/`Ctrl+Y` to undo/redo

## Misc info
- The fancy visualisation of the audio file in the editor isn't active for MP3 files. Well, OGG files are better anyway `¯\_(ツ)_/¯`
- Behind the scenes, a `song` has a folder with a `song.json` for shared data, and one or multiple `.drive` files, which are the difficulties. When loading a drive chart, you have to pick one of these `.drive` files.
- The `[Reload]` button relentlessly reloads the currently opened drive chart, discarding all changes made since the last time you `[Save]`d. Tread with care!
