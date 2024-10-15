# VerfioneSPRemotePurchaseTerminalIntegration

## Overview

The **VerfioneSPRemotePurchaseTerminalIntegration** project is designed to integrate a remote purchase terminal solution, certified by Verfione and compliant with SIBS standards. This integration uses IP communication with Verfione's terminals to facilitate remote purchase operations, with full support for certification and validation processes.

## Features

- **Terminal Status Requests**: Query the current status of the terminal.
- **Purchase Operations**: Send a purchase request to the terminal for remote processing.
- **Refund Operations**: Initiate refund operations through the terminal.
- **Period Management**: Open and close operational periods.
  
## Commands Supported

1. **Terminal Status Request**: Retrieves the current status of the terminal.
2. **Open Period**: Starts a new operational period on the terminal.
3. **Close Period**: Closes the current operational period on the terminal.
4. **Purchase**: Processes a purchase transaction by sending a transaction ID and amount.
5. **Refund**: Processes a refund transaction by sending a transaction ID and amount.

## Communication Protocol

- **Connection Type**: IP-based communication (Ethernet or Wi-Fi)
- **Ports**: The communication with the terminal happens on port `15200`.
- **Protocol**: TCP/IP with hexadecimal command structures.

## Setup

1. Clone the repository:
    ```bash
    git clone https://github.com/yourusername/VerfioneSPRemotePurchaseTerminalIntegration.git
    ```
   
2. Open the solution in Visual Studio.

3. Modify the terminal IP address if needed:
    ```csharp
    private const string serverIp = "192.168.1.252";  // Modify to match your terminal's IP address
    ```

4. Build and run the project.

## Usage

Once the application is running, you can interact with the terminal by selecting one of the available commands:

- Terminal Status
- Purchase
- Refund
- Open Period
- Close Period

To quit the application, enter 'q'.

## Requirements

- .NET Framework or .NET Core
- Access to a Verfione terminal
- IP-based connection (Ethernet or Wi-Fi)

## Support

For any questions or issues regarding this project, please contact the Verfione team or consult the provided documentation for further details.

