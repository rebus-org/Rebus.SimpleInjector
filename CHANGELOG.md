# Changelog

## 2.0.0-a1
* Test release

## 2.0.0-b01
* Test release

## 2.0.0
* Release 2.0.0

## 3.0.0
* Update to Rebus 3

## 4.0.0
* Update to Rebus 4
* Add .NET Core support (netstandard1.6)
* Circumvent SimpleInjector's reluctance to dispose singletons that have not been resolved
* Update contracts dep - thanks [trevorreeves]

## 5.0.0
* Change configuration API to work better with SimpleInjector
* Target .NET Core 2.0 too

## 6.0.0
* Update to Rebus 6 - thanks [GrzegorzBlok]

## 7.0.0
* Update to SimpleInjector 5 - thanks [Eshva]
* Use scopes to implement proper disposal of handlers (means that you need to remember to enable scopes in the container, e.g. by setting `Options = { DefaultScopedLifestyle = ScopedLifestyle.Flowing }`)


[Eshva]: https://github.com/Eshva
[GrzegorzBlok]: https://github.com/GrzegorzBlok
[trevorreeves]: https://github.com/trevorreeves
