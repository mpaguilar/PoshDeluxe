## This project is obsolete!

I wrote this for PowerShell 3.0. Later versions have built-in facilities for doing this, better. Keeping it up here for those poor folks that are stuck on old versions.

## PoshDeluxe

A general framework for running PowerShell scripts.

## What it's for

I'd been looking for ways to make various scripts easy to use. Even for a technical audience, using a script once or twice can be a little nerve-wracking, checking and double-checking the parameters. 

For my own use, I often needed a lot of little pieces of data refreshed, and displayed in an easy-to-read format. I was using a (probably usual) blob of custom scripts, import-into-this-then-run-that workflow.

PowerShell does provide some GUI capabilities, but I'd been finding them cumbersome after being spoiled by VisualStudio.

The answer, obviously, was to write a [PowerShell host application](https://msdn.microsoft.com/en-us/library/ee706563(v=vs.85).aspx).

Some of the immediate advantages include:
 * parallel execution with no special PowerShell syntax
 * script isolation
 * restricting script capabilities
 * providing a richer api for display use (async bindings, refreshable, etc.)
 * keeping the rich libraries and ease of PowerShell scripts

Most importantly, it's pretty easy to put together a good script with a nice front-end. It looks like it should be possible to use it as part of a web-server, if sufficient care is taken restricting the shell. At the moment, it's pretty wide-open, so I don't recommend it.

### FAQ

 1. Does it do "X"?
    Nope. Ask, and I'll see if I can add it. I'm trying to cover the simplest case, first: simple scripts, with no apparent external dependencies, followed by the simplest C# code possible.

### Guidelines

The overall design grew out of a pattern I'd been following when writing .ps1 files. 
Each unit of code would emit a stream, and that stream would be of PSObjects containing only the necessary data.

Rather than return generic PSObjects to .NET code, it was easiest to use objects defined in C#. This has gotten me pretty close to "easy" PowerShell and C#. At the very least, it's an easy place to start when I have to deliver something easy-to-use.

### How to use it

Right now, it's geared towards C# programmers, using Visual Studio. It is by no means a complete solution to anything.

Each .ps1 should have a corresponding class file, derived from BasePoshModule, which contains the various functions and classes used by that script.

The script should emit values of a type defined by that class.

There are two example script/classes, returning some basic disk and network information.

There are two example displays, a console and a WPF app.

