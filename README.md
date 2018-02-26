# µScheme
An unpretentious Scheme interpreter in C#

µScheme is a work-in-progress scheme interpreter aiming for [R6RS](http://www.r6rs.org) compatibility. It's main aims are:

* Easy C# interoperability
* As standard compliant as possible
* Out-of-the-box support for the [Unity](https://unity3d.com) game engine

## Current state
As of February 2018 it supports most core syntactic forms and [Tail Call Optimization](https://en.wikipedia.org/wiki/Tail_call). It evaluates expressions using a stack-based machine inspired by the one described in [SICP](https://mitpress.mit.edu/sicp/full-text/book/book-Z-H-34.html). I'm implementing new features slowly, and adding unit tests (using [NUnit](http://nunit.org/)) for every new addition.

It's organized as a Visual Studio 2017 solution, with a main project (uscheme) and a test project (uscheme-test). The code has no dependencies outside of the NUnit framework for the test project, so it should be easy to just open it from Visual Studio or other IDEs that support the format.
