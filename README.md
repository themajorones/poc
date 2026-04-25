# PoC Android App

A generic Android proof-of-concept application for business analysts, with automated build and release workflow.

## Project Structure

```
poc/
├── app/                           # Main application module
│   ├── src/main/
│   │   ├── java/com/poc/app/      # Java source code
│   │   │   └── MainActivity.java
│   │   ├── res/                   # Android resources
│   │   │   ├── layout/
│   │   │   │   └── activity_main.xml
│   │   │   ├── values/
│   │   │   │   ├── colors.xml
│   │   │   │   ├── strings.xml
│   │   │   │   └── themes.xml
│   │   └── AndroidManifest.xml
│   ├── build.gradle
│   └── proguard-rules.pro
├── gradle/
│   └── wrapper/                   # Gradle wrapper
├── .github/
│   └── workflows/
│       └── build.yml              # GitHub Actions workflow
├── build.gradle
├── settings.gradle
├── gradlew                        # Gradle wrapper script (Linux/Mac)
├── gradlew.bat                    # Gradle wrapper script (Windows)
└── README.md
```

## Build Requirements

- Java 11 or higher
- Android SDK (API level 24-34)
- Gradle (included via wrapper)

## Local Build

### On Linux/Mac:
```bash
chmod +x gradlew
./gradlew assembleRelease  # Build APK
./gradlew bundleRelease    # Build AAB
```

### On Windows:
```bash
gradlew.bat assembleRelease  # Build APK
gradlew.bat bundleRelease    # Build AAB
```

### Build Outputs
- **APK**: `app/build/outputs/apk/release/app-release.apk`
- **AAB**: `app/build/outputs/bundle/release/app-release.aab`

## GitHub Actions Workflow

The build workflow is defined in `.github/workflows/build.yml` and is configured to:

- **Trigger**: Only on pushes to the `workflow` branch (branch-specific)
- **Steps**:
  1. Checkout code
  2. Set up JDK 11
  3. Build APK (release flavor)
  4. Build AAB (release flavor)
  5. Upload both artifacts to GitHub

### Artifacts

Built artifacts are automatically uploaded to GitHub Actions and retained for 30 days:
- `app-release-apk` - The signed release APK
- `app-release-bundle` - The Android App Bundle

### Viewing Artifacts

1. Go to the GitHub repository
2. Navigate to **Actions** → **Build and Upload APK**
3. Select the workflow run
4. Download artifacts from the "Artifacts" section

## Configuration

### Target Android Versions

Edit `build.gradle` root level to modify SDK versions:
```gradle
ext {
    compileSdkVersion = 34      # Compile target
    buildToolsVersion = '34.0.0'
    minSdkVersion = 24          # Minimum supported version
    targetSdkVersion = 34       # Target version
}
```

### Application ID

Edit `app/build.gradle` to change the app ID:
```gradle
applicationId "com.poc.app"  # Change to your package name
```

## Development

### Project Configuration Files

- `build.gradle` - Root Gradle configuration
- `app/build.gradle` - App module configuration
- `settings.gradle` - Project structure definition
- `gradle.properties` - (Optional) Gradle properties

### Customization

1. **App Name**: Edit `app/src/main/res/values/strings.xml`
2. **App Icon**: Replace icons in `app/src/main/res/mipmap-*` directories
3. **Styling**: Modify `app/src/main/res/values/themes.xml`
4. **ProGuard Rules**: Update `app/proguard-rules.pro` for production builds

## Workflow Rules

The GitHub Actions workflow applies **only to the `workflow` branch**. This ensures:

- Builds are isolated from the main branch
- Only intended branch triggers automated builds
- Artifacts are generated specifically for this PoC

To apply workflow to other branches, update the `branches:` configuration in `.github/workflows/build.yml`:

```yaml
on:
  push:
    branches:
      - workflow
      - your-other-branch  # Add additional branches
```

## Notes for Business Analysts

- **APK**: Install directly on Android devices/emulators
- **AAB**: Required for Google Play Store distribution
- **Artifacts**: Available in GitHub Actions after each build
- **Build Time**: Typically 2-5 minutes depending on system

## License

This is a proof-of-concept application for evaluation purposes.
