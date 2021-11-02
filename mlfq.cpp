#include <queue>
#include <windows.h>
#include <stdio.h>
#include <iostream>
#include <string>

using namespace std;

HANDLE gDoneEvent1;
HANDLE gDoneEvent2;
HANDLE gDoneEvent3;
HANDLE gDoneEvent4;
HANDLE gDoneEvent5;


void callSetEvent(int value)
{
    if (value == 1)
    {
        SetEvent(gDoneEvent1);
    }
    else if (value == 2)
    {
        SetEvent(gDoneEvent2);
    }
    else if (value == 3)
    {
        SetEvent(gDoneEvent3);
    }
    else if (value == 4)
    {
        SetEvent(gDoneEvent4);
    }
    else if (value == 5)
    {
        SetEvent(gDoneEvent5);
    }
    
}

VOID CALLBACK TimerRoutine(PVOID lpParam, BOOLEAN TimerOrWaitFired)
{
    if (lpParam == NULL)
    {
        printf("TimerRoutine lpParam is NULL\n");
    }
    else
    {
        int* ptr = (int*)lpParam;
        //printf("Timer routine called. Parameter is %d.\n",
        //    *(int*)lpParam);
        printf("Timer routine called. Parameter is %d.\n",*ptr);
        ptr++;
        printf("Timer routine called. Parameter is %d.\n", *ptr);

        if (TimerOrWaitFired)
        {
            printf("The wait timed out.\n");
        }
        else
        {
            printf("The wait event was signaled.\n");
        }
    }
    callSetEvent(1);
}



//--------------------------------------------EVENT--------------------------------------------

void createEventQueue(HANDLE& hTimerQueue)
{
    // Create the timer queue.
    hTimerQueue = CreateTimerQueue();

    if (NULL == hTimerQueue)
    {
        printf("CreateTimerQueue failed (%d)\n", GetLastError());
    }
}
void createEvent(HANDLE& doneEvent)
{
    doneEvent = CreateEvent(NULL, TRUE, FALSE, NULL);

    if (NULL == doneEvent)
    {
        printf("CreateEvent failed (%d)\n", GetLastError());
    }
}
void waitSignalEvent(HANDLE& doneEvent, string message)
{
    if (WaitForSingleObject(gDoneEvent1, INFINITE) != WAIT_OBJECT_0)
    {
        printf("WaitForSingleObject failed (%d)\n", GetLastError());
    }
    else
    {

    }
}
//----------------------------------------------------------------------------------------

void addQueueTimer(HANDLE& timer, HANDLE& timerQueue, int& args, int milisecondForTimer)
{
    // Set a timer to call the timer routine in 10 seconds.
    if (!CreateTimerQueueTimer(&timer, timerQueue, (WAITORTIMERCALLBACK)TimerRoutine, &args, 4000, 0, 0))
    {
        printf("CreateTimerQueueTimer failed (%d)\n", GetLastError());
    }
}


int main()
{
    HANDLE hTimer1 = NULL;
    HANDLE hTimer2 = NULL;
    HANDLE hTimer3 = NULL;
    HANDLE hTimer4 = NULL;
    HANDLE hTimer5 = NULL;
    HANDLE hTimer6 = NULL;
    HANDLE hTimer7 = NULL;
    HANDLE hTimer8 = NULL;
    
    HANDLE hTimerQueue = NULL;

    int arg[3] = { 53,21,32 };
    int arg1 = 444;
    int arg2 = 555;


    queue<string> myqueue1;
    queue<string> myqueue2;
    queue<string> myqueue3;
    queue<string> myqueue4;
    queue<string> myqueue5;
    queue<string> myqueue6;
    queue<string> myqueue7;
    queue<string> myqueue8;
    queue<string> queueArray[8] = { myqueue1,myqueue2,myqueue3,myqueue4,myqueue5,myqueue6,myqueue7,myqueue8 };
    queue<string>* ptrQueue;
    ptrQueue = queueArray;



    // Use an event object to track the TimerRoutine execution
    
    createEvent(gDoneEvent1);
    createEvent(gDoneEvent2);
    createEvent(gDoneEvent3);

    createEventQueue(hTimerQueue);


    if (!CreateTimerQueueTimer(&hTimer1, hTimerQueue, (WAITORTIMERCALLBACK)TimerRoutine, &arg, 2000, 0, 0))
    {
        printf("CreateTimerQueueTimer failed (%d)\n", GetLastError());
        return 3;
    }

    // TODO: Do other useful work here 

    printf("Call timer routine in 10 seconds...\n");

    // Wait for the timer-queue thread to complete using an event 
    // object. The thread will signal the event at that time.

    waitSignalEvent(gDoneEvent1,"");
    CloseHandle(gDoneEvent1);

    // Delete all timers in the timer queue.
    if (!DeleteTimerQueue(hTimerQueue))
        printf("DeleteTimerQueue failed (%d)\n", GetLastError());

    return 0;
}