// Serial protocol definitions
// FIRST BYTE: 0 - Channel A; 1 - Channel B
// SECOND BYTE: Command to be executed
// THIRD BYTE: 0 - Do not repeat; 1 - Repeat
// FOURTH BYTE: Checksum - XOR of other bytes
#define SERIAL_BUF_LEN 4
#define CHANNEL_BYTE 0
#define COMMAND_BYTE 1
#define REPEAT_BYTE 2
#define CHECKSUM_BYTE 3

// Command offsets
#define CHANNEL_OFFSET 21
#define CHECKSUM_OFFSET 16
#define COMMAND_OFFSET 8

// ------------------ - info about bits------------------------
#define totallength 22 //number of highs/bits 4 channel + 18 command
#define channelstart 0
#define commandstart 4 //bit where command starts
#define channellength 4
#define commandlength 18
//---------determined empirically--------------
#define headerlower 2300 //lower limit
#define headernom 2550 //nominal
#define headerupper 2800 //upper limit
#define zerolower 300
#define zeronom 500 //380 //nominal
#define zeroupper 650
#define onelower 800
#define onenom 1000//850 //nominal
#define oneupper 1100
#define highnom 630

//---------------------pin assignments--------------
#define TXpin 7
#define RXpin 2 //doesnt use interrupts so can be anything

//----------------------variables----------------------
#define countin 1048576
boolean bit2[totallength];
unsigned long buttonnum;
char msg = ' ';
unsigned long x = 0;
unsigned long count = countin;
unsigned long buf = 0;

typedef struct serialCommand {		// Structure to store the state of a channel
	unsigned long command;			// Command to be sent
	boolean valid;					// Is the current data valid?
	boolean repeat;					// Should the command be repeated?
} serialCommand;

byte serialBuf[SERIAL_BUF_LEN];		// Buffer for serial input
serialCommand isobotState[2];		// Array of state structures, one for each channel

// Initial setup of device
void setup()
{
	Serial.begin(38400);
	pinMode(RXpin, INPUT);
	pinMode(TXpin, OUTPUT);
	isobotState[0].valid = false;
	isobotState[1].valid = false;
}


void loop()
{
	for (int i = 0; i < 2; ++i)
	{
		if (isobotState[i].valid)
		{
			buttonwrite(TXpin, isobotState[i].command);

			if (isobotState[i].repeat == false)
			{
				isobotState[i].valid = false;
			}
		}
	}
}

// Handler for incoming serial data
void serialEvent()
{
	while (Serial.available())
	{
		Serial.readBytes(serialBuf, SERIAL_BUF_LEN);

		// Check for valid checksum
		if (serialBuf[CHANNEL_BYTE] ^ serialBuf[COMMAND_BYTE] ^ serialBuf[REPEAT_BYTE] ^ serialBuf[CHECKSUM_BYTE])
			continue;

		// Check for valid channel
		unsigned long channel = serialBuf[CHANNEL_BYTE];	// Select channel
		if (channel > 1 || channel < 0)
			continue;

		unsigned long command = serialBuf[COMMAND_BYTE];	// Command word
		boolean repeat = serialBuf[REPEAT_BYTE] ? true : false;			// Should the command be repeated?

		// Check for stop repeating command
		if (command == 0x00)
		{
			isobotState[channel].repeat = false;
			isobotState[channel].valid = false;
			continue;
		}


		// Build command word without checksum
		command = command << COMMAND_OFFSET;
		command |= (channel << CHANNEL_OFFSET);
		command |= 0x80003;

		// Calculate checksum
		unsigned long checksum1 = ((command >> 16) & 0xFF) + ((command >> 8) & 0xFF) + (command & 0xFF);
		unsigned long checksum2 = 0;
		for (int i = 0; i < 3; ++i)
		{
			checksum2 += checksum1 & 0x7;
			checksum1 = checksum1 >> 3;
		}

		// Add checksum to command word
		isobotState[channel].command = command + ((checksum2 & 0x7) << CHECKSUM_OFFSET);

		// Setup for next execution of main loop
		isobotState[channel].repeat = repeat;
		isobotState[channel].valid = true;
	}
}

void ItoB(unsigned long integer, int length)
{
    //needs bit2[length]
    Serial.println("ItoB");
    for (int i = 0; i < length; i++)
    {
        if ((integer / power2(length - 1 - i)) == 1)
        {
            integer -= power2(length - 1 - i);
            bit2[i] = 1;
        }
        else bit2[i] = 0;
        Serial.print(bit2[i]);
    }
    Serial.println();
}

unsigned long power2(int power)
{ //gives 2 to the (power)
    unsigned long integer = 1; //apparently both bitshifting and pow functions had problems
    for (int i = 0; i < power; i++)
    { //so I made my own
        integer *= 2;
    }
    return integer;
}

void buttonwrite(int txpin, unsigned long integer)
{
    //must be full integer (channel + command)
    ItoB(integer, 22); //must have bit2[22] to hold values
    oscWrite(txpin, headernom);
    for (int i = 0; i < totallength; i++)
    {
        if (bit2[i] == 0) delayMicroseconds(zeronom);
        else delayMicroseconds(onenom);
        oscWrite(txpin, highnom);
    }
    delay(205);
}

void oscWrite(int pin, int time)
{ //writes at approx 38khz
    for (int i = 0; i < (time / 26) - 1; i++)
    {
        //prescaler at 26 for 16mhz, 52 at 8mhz, ? for 20mhz
        digitalWrite(pin, HIGH);
        delayMicroseconds(10);
        digitalWrite(pin, LOW);
        delayMicroseconds(10);
    }
}

