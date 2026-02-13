# Zed.EntityFrameworkCore

[![Zed.EntityFrameworkCore on nuget.org](https://img.shields.io/nuget/v/Zed.EntityFrameworkCore.svg)](https://www.nuget.org/packages/Zed.EntityFrameworkCore) [![Zed.EntityFrameworkCore on fuget.org](https://www.fuget.org/packages/Zed.EntityFrameworkCore/badge.svg)](https://www.fuget.org/packages/Zed.EntityFrameworkCore)

Zed.EntityFrameworkCore is a library for Entity Framework Core support

## CI build status

| Branch  | Build status                                                                                                                                                                                     |
| ------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Master  | [![Main](https://github.com/ztepsic/zed.entityframeworkcore/actions/workflows/main.yml/badge.svg)](https://github.com/ztepsic/zed.entityframeworkcore/actions/workflows/main.yml)                |
| Develop | [![Main](https://github.com/ztepsic/zed.entityframeworkcore/actions/workflows/main.yml/badge.svg?branch=develop)](https://github.com/ztepsic/zed.entityframeworkcore/actions/workflows/main.yml) |

## Dev setup

### [Conventional commits](https://www.conventionalcommits.org/)

- [Commit Lint - Lint commit messages](https://commitlint.js.org/)
  - Intall locally
    ```sh
        npm install --save-dev husky
        npx husky init
        echo "npx --no -- commitlint --edit \$1" > .husky/commit-msg
    ```
- [Commitizen](http://commitizen.github.io/cz-cli/)

  - Install locally

    ```sh
    npm install --save-dev commitizen
    npm install --save-dev @commitlint/cz-commitlint commitizen inquirer@9
    ```

### Versioning with [GitVersion](https://gitversion.net/)

- Install .net global tool

  ```sh
  dotnet tool install --global GitVersion.Tool
  ```

- Run

  ```sh
  dotnet-gitversion
  ```

- [Arguments](https://gitversion.net/docs/usage/cli/arguments)
