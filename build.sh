#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

die() {
    echo "ERREUR: $*" >&2
    exit 1
}

require_command() {
    command -v "$1" >/dev/null 2>&1 || die "commande introuvable : $1"
}

detect_ksp_data_dir() {
    if [[ -z "${KSPDIR:-}" ]]; then
        die "la variable d'environnement KSPDIR n'est pas définie (répertoire d'installation de KSP)"
    fi

    if [[ -f "$KSPDIR/KSP_x64_Data/Managed/Assembly-CSharp.dll" ]]; then
        echo "Structure Windows détectée (KSP_x64_Data)"
        KSP_DATA_DIR="$KSPDIR/KSP_x64_Data"
    elif [[ -f "$KSPDIR/KSP_Data/Managed/Assembly-CSharp.dll" ]]; then
        echo "Structure Linux détectée (KSP_Data)"
        KSP_DATA_DIR="$KSPDIR/KSP_Data"
    else
        die "Assembly-CSharp.dll introuvable dans $KSPDIR/KSP_x64_Data/Managed/ ou $KSPDIR/KSP_Data/Managed/"
    fi

    echo "Utilisation de KSPDIR: $KSPDIR"
    echo "Utilisation de KSP_DATA_DIR: $KSP_DATA_DIR"
}

check_dependencies() {
    if [[ ! -f "$KSPDIR/GameData/000_ClickThroughBlocker/Plugins/ClickThroughBlocker.dll" ]]; then
        die "Click Through Blocker introuvable dans GameData (requis pour la compilation)"
    fi
}

echo "========="
echo "Building"
echo "========="

require_command dotnet
require_command zip
detect_ksp_data_dir
check_dependencies

MSBUILD_PROPS=(-p:KSPDIR="$KSPDIR" -p:KSP_DATA_DIR="$KSP_DATA_DIR")

echo "Suppression du dossier Release"
rm -rf Release

echo "Création du dossier Release"
mkdir -p Release/VesselBookmarkMod/{vessel_types,buttons,Textures,Localization}

echo "Restauration des packages NuGet"
dotnet restore VesselBookmark.sln "${MSBUILD_PROPS[@]}"

echo "Compilation de la DLL du mod (.NET Framework 4.7.2)"
dotnet build VesselBookmark.sln "${MSBUILD_PROPS[@]}" --no-restore

echo "Copie de la DLL du mod"
cp -v Output/bin/VesselBookmarkMod.dll Release/VesselBookmarkMod/

echo "Copie du fichier de configuration ModuleManager"
cp -v GameData/VesselBookmarkMod/VesselBookmarkMod.cfg Release/VesselBookmarkMod/

echo "Copie des icônes"
cp -v GameData/VesselBookmarkMod/*.png Release/VesselBookmarkMod/

echo "Copie des icônes de types de vaisseau"
cp -v GameData/VesselBookmarkMod/vessel_types/* Release/VesselBookmarkMod/vessel_types/

echo "Copie des icônes de boutons"
cp -v GameData/VesselBookmarkMod/buttons/* Release/VesselBookmarkMod/buttons/

# Sprites TMP (rendus inline via <sprite> dans les labels) : icônes partagées (refresh_icon) +
# icônes propres au mod (edit/goto/target). Lues à l'exécution depuis GameData/.../Textures.
echo "Copie des textures partagées (sprites TMP)"
cp -v KSP-Shared/GameData/Textures/* Release/VesselBookmarkMod/Textures/

echo "Copie des textures du mod (sprites TMP)"
if compgen -G "GameData/VesselBookmarkMod/Textures/*.png" > /dev/null; then
    cp -v GameData/VesselBookmarkMod/Textures/*.png Release/VesselBookmarkMod/Textures/
else
    echo "  (aucune texture propre au mod pour l'instant)"
fi

echo "Copie des fichiers de localisation"
cp -v GameData/VesselBookmarkMod/Localization/* Release/VesselBookmarkMod/Localization/

echo "Création de l'archive"
(
    cd Release/VesselBookmarkMod
    zip -qr ../VesselBookmarkMod.zip .
)

echo "Suppression du dossier intermédiaire"
rm -rf Release/VesselBookmarkMod

echo
echo "Build terminé : Release/VesselBookmarkMod.zip"
echo "Exécuté le : $(date)"
