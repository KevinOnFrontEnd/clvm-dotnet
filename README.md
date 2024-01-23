This repository is a dotnet port of Chia-Networks CLVM located at https://github.com/Chia-Network/clvm. The aim of this project is to allow Chialisp programs to be run using dotnet without installing node or python on your server.

![Build & Test](https://github.com/KevinOnFrontEnd/clvm-dotnet/actions/workflows/build_and_test.yml/badge.svg)

**This repo is not fully complete and requires further porting**

# Progress
The bulk of the work has been completed in getting the CLVM working. Internally it has the binary tree has been represented and can do operations on the tree. The remaining work is wiring up what the compile command does by iterating over the source and building the tree.

| python class | Comment   |
| :---:   | :---: |
| clvmc | Assebles the binary tree (SExp object) and runs the program (WIP) |
| curry   | currys parameters into the program |
| uncurry   | uncurrys parameters into the program |


# Installation instructions
A Nuget package will be published when this repository has successfully migrated the clvm.

# Usage
TBC





