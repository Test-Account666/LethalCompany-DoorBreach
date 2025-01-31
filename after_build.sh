rm -rf BuildOutput &&
mkdir BuildOutput &&
cp -f "$CURRENT_PROJECT"/bin/Debug/netstandard2.1/TestAccount666."$CURRENT_PROJECT".dll BuildOutput/"$CURRENT_PROJECT".dll &&
cp -f "$CURRENT_PROJECT"/README.md BuildOutput/ &&
cp -f "$CURRENT_PROJECT"/CHANGELOG.md BuildOutput/ &&
cp -f "$CURRENT_PROJECT"/icon.png BuildOutput/ &&
cp -rf DoorSfx BuildOutput/ &&
cp -f LICENSE BuildOutput/ &&
cp -f AssetBundle/doorbreach BuildOutput/ &&
./generate_manifest.sh &&
./generate_zipfile.sh &&
dolphin "./BuildOutput"
