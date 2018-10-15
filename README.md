# Chihuahua
A tool and library for working with the event data from Luigi's Mansion and its 3DS remake.

Beverly is a .NET Core tool that allows the user to dump Luigi's Mansion 3D's binary event files (.bev) back to the original game's plain-text format (.txt), and vice versa.

Chihuahua is the library for doing this, taking in a file, parsing it into an easily-accessible data structure, and exposing facilities to write that structure back to either the 3DS remake's .bev format or the original game's .txt format.