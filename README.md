# AppManager

A comprehensive application management tool built with .NET 8 and WPF that allows users to monitor, control, and automate applications through triggers and actions.

## Introduction

! UNDER CONSTRUCTION !\
This document, except Introduction, was made by these 2 promts to Copilot:

###

Describe the store data and edit data functionality as the user experience it.

###

Now add my question and your answer to a proper named readme file. If it can be read on github as well, with out too much hassle, that would be great.

## Overview

AppManager provides users with the ability to manage applications through configurable triggers (keyboard shortcuts, app events, system events) and corresponding actions (launch, close, focus, etc.). The application features a persistent profile system that saves user configurations and preferences.

## Features

- **Application Management**: Add, configure, and monitor applications
- **Group Management**: Organize applications into logical groups
- **Trigger System**: Configure various trigger types (shortcuts, app launch/close, system events)
- **Action System**: Define actions to execute when triggers activate
- **Persistent Storage**: Automatic saving and loading of user configurations
- **Profile Management**: User-specific settings and window state persistence

## Data Management System

### Store Data and Edit Data Functionality - User Experience

#### **Data Storage System**

The AppManager application provides users with a persistent data storage system that automatically manages their application configurations:

**Profile-Based Storage:**
- User data is stored in a JSON file (`AppsManaged.json`) located in the user's AppData folder (`%APPDATA%\AppManager\`)
- The system automatically creates a default profile for new users with their Windows username
- All configurations persist between application sessions

#### **Data Editing User Experience**

##### **1. Navigation and Item Management**
**Adding New Items:**
- Users can quickly add new apps or groups through text input fields in the navigation panel
- Simply type the name and press Enter to create a new item
- Items are immediately available for configuration

**Browsing Items:**
- Clean, organized navigation with three main sections: Apps, Groups, and Shortcuts
- Left sidebar shows all created items as clickable buttons
- Selected items highlight the current editing context

##### **2. App Configuration Editing**

When users click on an app from the navigation, they see a dedicated editing interface:

**Basic App Settings:**
- **App Name**: Editable text field for the application identifier
- **Active Status**: Checkbox to enable/disable the app's management

**Advanced Configuration:**
- **Triggers Section**: Visual list showing configured triggers (shortcuts, app events, etc.)
- **Actions Section**: Display of actions that will be performed when triggers activate
- Easy-to-use "Add Trigger" and "Add Action" buttons for expanding functionality

##### **3. Real-Time Editing Experience**

**Immediate Feedback:**
- Changes are reflected instantly in the UI as users type or interact with controls
- Visual indicators show when data has been modified but not yet saved

**Change Tracking:**
- The system tracks unsaved changes using an `IsStored` flag
- Backup models preserve the original state for potential rollback

##### **4. Data Persistence Controls**

**Save Functionality:**
- Dedicated "Save" button commits all changes to the profile file
- Success messages confirm when data has been saved
- Version information is automatically updated with each save

**Cancel/Rollback:**
- "Cancel" button allows users to discard unsaved changes
- System restores the previous saved state from backup models
- Confirmation messages indicate successful data restoration

##### **5. Group Management Experience**

For app groups, users get enhanced editing capabilities:

**Dynamic UI Generation:**
- Rich editing interface with description fields, member management, and settings
- **Selected**: Checkbox for group activation
- **Auto Close All**: Option to close all member apps together
- **Expanded**: Controls group display state

**Member App Management:**
- Visual list showing all apps in the group
- Add/Remove buttons with dialog interfaces for selecting apps
- Real-time member count display

##### **6. Window State Persistence**

The application remembers user preferences:
- Window size, position, and maximized state
- Last selected navigation menu and item
- Theme preferences and scanning intervals

##### **7. Error Handling and Safety**

**Graceful Error Management:**
- Automatic creation of default profiles if data files are missing or corrupted
- Debug logging for troubleshooting without user disruption
- Fallback mechanisms ensure the application remains functional

#### **User Workflow Summary**

1. **Navigate** → Click on Apps, Groups, or Shortcuts in the top navigation
2. **Select/Create** → Choose an existing item or create a new one using the text input
3. **Edit** → Modify settings in the main editing panel with immediate visual feedback
4. **Configure** → Add triggers and actions using dedicated buttons and interfaces
5. **Persist** → Use Save to commit changes or Cancel to discard modifications
6. **Verify** → Receive confirmation messages for successful operations

This design provides users with an intuitive, safe, and efficient way to manage their application configurations while ensuring data persistence and offering robust editing capabilities with proper change tracking and rollback functionality.

## Technical Architecture

### Core Components

- **ProfileData**: Main data model containing user settings and application configurations
- **AppManagedModel**: Individual application configuration with triggers and actions
- **GroupManagedModel**: Application group management
- **TriggerSystem**: Handles various trigger types (shortcuts, app events, system events)
- **ActionSystem**: Executes configured actions (launch, close, focus, etc.)

### Technologies Used

- **.NET 8**: Core framework
- **WPF**: User interface framework
- **System.Text.Json**: Data serialization
- **Win32 APIs**: System integration and global keyboard hooks

## Getting Started

### Prerequisites

- .NET 8 Runtime or SDK
- Windows 10/11

### Installation

1. Clone the repository:
