# Feliz.Recoil - Installation

To install `Feliz.Recoil` into your project, you need to install the nuget package into your F# project
```bash
# nuget
dotnet add package Feliz.Recoil
# paket
paket add Feliz.Recoil --project ./project/path
```
Then you need to install the corresponding npm dependencies. In case of Feliz.Recoil, it is just `recoil`.
```bash
npm install recoil

___

yarn add recoil
```

### Use Femto

If you happen to use [Femto](https://github.com/Zaid-Ajaj/Femto), then it can install everything for you in one go
```bash
cd ./project
femto install Feliz.Recoil
```
Here, the nuget package will be installed using the package manager that the project is using (detected by Femto) and then the required npm packages will be resolved
