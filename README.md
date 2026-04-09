# Edutech: AI-Powered AR Vision Engine

Edutech is a high-fidelity Augmented Reality (AR) application that leverages Google's Gemini AI to provide real-time object detection, technical analysis, and spatial learning experiences. 

![Edutech Banner](https://img.shields.io/badge/Unity-2022.3%2B-blue?logo=unity)
![Gemini AI](https://img.shields.io/badge/AI-Gemini_2.5_Flash-orange)
![AR Foundation](https://img.shields.io/badge/AR-AR_Foundation-green)

## 🚀 Features

- **Spatial Holographic UI**: Immersive sci-fi themed HUD elements anchored to real-world objects.
- **Multimodal AI Analysis**: Uses Gemini 2.5 Flash to identify objects and provide technical, creative, or beginner-level educational content.
- **Real-time Object Detection**: Integrated TFLite/Gemini hybrid pipeline for precise spatial anchoring.
- **Voice Guidance**: Integrated Text-to-Speech (TTS) for hands-free learning.
- **Privacy First**: Secure camera initialization with built-in privacy safeguards.

## 🛠️ Setup Instructions

### Prerequisites
- **Unity 2022.3** or higher.
- **AR Foundation** compatible device (Android with ARCore or iOS with ARKit).
- **Google Gemini API Key**: Get one for free at [Google AI Studio](https://aistudio.google.com/app/apikey).

### Installation

1. **Clone the repository**:
   ```bash
   git clone https://github.com/your-username/edutech.git
   ```

2. **Configure API Key**:
   - Rename `.env.example` to `.env` in the project root.
   - Open `.env` and paste your Gemini API Key:
     ```env
     GEMINI_API_KEY=your_actual_key_here
     ```

3. **Open in Unity**:
   - Add the project to your Unity Hub and open it.
   - Let Unity re-import assets (this may take a few minutes as the `Library` folder is excluded).

4. **Build for Android/iOS**:
   - Ensure you have the necessary Build Support modules installed.
   - Select **Landscape Left** as the default orientation.
   - For Android, ensure **Vulkan** is removed from the Graphics APIs (OpenGLES3 is required for ARCore).

## 📂 Project Structure

- `/Assets/Scripts/AI`: Core AI logic including Gemini API connectors and object detection.
- `/Assets/Scripts/UI`: Custom holographic UI and HUD management.
- `/Assets/Scripts/Core`: Application bootstrap and data persistence.
- `/ProjectSettings`: Pre-configured Unity project settings for AR.

## 🛡️ License

Distributed under the MIT License. See `LICENSE` for more information.

---
*Built with ❤️ for the future of education.*
