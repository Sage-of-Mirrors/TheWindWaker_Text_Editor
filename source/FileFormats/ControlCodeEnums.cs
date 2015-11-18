using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextEditor2
{
    //Control codes in TWW are signaled by a byte value
    //of 0x1A/26 dec. This is followed by the size of the code.
    public enum ControlCodeSizes : byte
    {
        //Control codes have three possible sizes.

        //Most common, used mostly for inserting icons or variable char data
        //Also includes dynamic drawing mode switching
        FiveBytes = 0x5,

        //Used for changing text color
        SixBytes = 0x6,

        //Used for things that affect the text, like pausing drawing
        //or setting text size
        SevenBytes = 0x7
    }

    //Control code types determine what the control code does.
    //For FiveByte types, the last byte of the code determines
    //what it does. But for some it does change. An example is the FiveByte type
    //PlaySound, which has a variable in the spot that the other
    //FiveByte types use for their identifier; thus the type byte
    //for PlaySound is actually right after the size, instead of
    //in the last byte of the code.

    public enum FiveByteTypes : byte
    {
        PlayerName = 0x00,

        CharDrawInstant = 0x01, //PlaySound = 0x01, but in a different field within the code

        CharDrawByChar = 0x02,

        //0x03 through 0x07 aren't used

        TwoChoices = 0x08,

        ThreeChoices = 0x09,

        AButtonIcon = 0x0A,

        BButtonIcon = 0x0B,

        CStickIcon = 0x0C,

        LTriggerIcon = 0x0D,

        RTriggerIcon = 0x0E,

        XButtonIcon = 0x0F,

        YButtonIcon = 0x10,

        ZButtonIcon = 0x11,

        DPadIcon = 0x12,

        StaticControlStickIcon = 0x13,

        LeftArrowIcon = 0x14,
        
        RightArrowIcon = 0x15,

        UpArrowIcon = 0x16,

        DownArrowIcon = 0x17,

        ControlStickMovingUp = 0x18,

        ControlStickMovingDown = 0x19,

        ControlStickMovingLeft = 0x1A,

        ControlStickMovingRight = 0x1B,

        ControlStickMovingUpAndDown = 0x1C,

        ControlStickMovingLeftAndRight = 0x1D,

        ChoiceOne = 0x1E,

        ChoiceTwo = 0x1F,

        CanonGameBalls = 0x20,

        BrokenVasePayment = 0x21,

        AuctionCharacter = 0x22,

        AuctionItemName = 0x23,

        AuctionPersonBid = 0x24,

        AuctionStartingBid = 0x25,

        PlayerAuctionBidSelector = 0x26,

        StarburstAIcon = 0x27,

        OrcaBlowCount = 0x28,

        PirateShipPassword = 0x29,

        TargetStarburstIcon = 0x2A,

        PostOfficeGamePlayerLetterCount = 0x2B,

        PostOfficeGameRupeeReward = 0x2C,

        PostBoxLetterCount = 0x2D,

        RemainingKoroks = 0x2E,

        RemainingForestWaterTime = 0x2F,

        FlightPlatformGameTime = 0x30,

        FlightPlatformGameRecord = 0x31,

        BeedlePointCount = 0x32,

        JoyPendantCountMsMarie = 0x33,

        MsMariePendantTotal = 0x34,

        PigGameTime = 0x35,

        SailingGameRupeeReward = 0x36,

        CurrentBombCapacity = 0x37,

        CurrentArrowCapacity = 0x38,

        HeartIcon = 0x39,

        MusicNoteIcon = 0x3A,

        TargetLetterCount = 0x3B,

        FishmanHitCount = 0x3C,

        FishmanRupeeReward = 0x3D,

        BokoBabaSeedCount = 0x3E,

        SkullNecklaceCount = 0x3F,

        ChuJellyCount = 0x40,

        JoyPendantCountBeedle = 0x41,

        GoldenFeatherCount = 0x42,

        KnightsCrestCount = 0x43,
         
        BeedlePriceOffer = 0x44,

        BokoBabaSeedSellSelector = 0x45,

        SkullNecklaceSellSelector = 0x46,

        ChuJellySellSelector = 0x47,

        JoyPendantSellSelector = 0x48,

        GoldenFeatherSellSelector = 0x49,

        KnightsCrestSellSelector = 0x4A
    }

    //There is only one SixByte Type - SetTextColor.
    public enum SixByteTypes : byte
    {
        SetTextColor = 0x0 //Includes 0xFF in the byte after Size
    }

    //SevenByte types are like SixByte types in structure,
    //except SevenBytes can store variables in a short
    //rather than a byte.
    public enum SevenByteTypes : byte
    {
        SetTextSize = 0x01, //Includes 0xFF in the byte after Size

        WaitAndDismissWithPrompt = 0x03,

        WaitAndDismiss = 0x04,

        Dismiss = 0x05,

        Dummy = 0x06,

        Wait = 0x07
    }
}
