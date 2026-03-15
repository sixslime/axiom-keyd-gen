{
    description = "axiom-keyd-gen";

    inputs = {
        nixpkgs.url = "github:NixOS/nixpkgs/nixos-25.11";
        flake-utils.url = "github:numtide/flake-utils";
    };

    outputs = { self, nixpkgs, flake-utils }:
    flake-utils.lib.eachDefaultSystem (system:
        let
            pkgs = nixpkgs.legacyPackages.${system};
        in {
            packages.default = pkgs.buildDotnetModule {
                pname = "axiom-keyd-gen";
                version = "0.1.0";
                src = ./.;

                projectFile = "SixSlime.AxoimKeydGen.csproj";
                nugetDeps = ./deps.nix;

                dotnet-sdk = pkgs.dotnetCorePackages.sdk_8_0;

                selfContainedBuild = true;
            };

            devShells.default = pkgs.mkShell {
                packages = [ pkgs.dotnetCorePackages.sdk_8_0 ];
            };
        });
}