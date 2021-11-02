#include <queue>
#include <windows.h>
#include <stdio.h>
#include <iostream>
#include <string>
#include <fstream>

using namespace std;

HANDLE gDoneRestEvent;
HANDLE gDoneEvent1;
HANDLE gDoneEvent2;
HANDLE gDoneEvent3;
HANDLE gDoneEvent4;



struct processStruct {
    queue<string> processValues;
    int priority;
    int state; //10 - init ,11- "0" ,12- "1",13 - "-" karsilik
};

VOID CALLBACK TimerRoutineRest(PVOID lpParam, BOOLEAN TimerOrWaitFired)
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
            //printf("The wait timed out.\n");
        }
        else
        {
            //printf("The wait event was signaled.\n");
        }
    }
    SetEvent(gDoneRestEvent);    
}


VOID CALLBACK TimerRoutine1(PVOID lpParam, BOOLEAN TimerOrWaitFired)
{
    if (lpParam == NULL)
    {
        printf("TimerRoutine lpParam is NULL\n");
    }
    else
    {
        queue<processStruct> q = *(queue<processStruct>*) lpParam;
        if (q.empty())
        {
            printf("OOOO111 LLLLL\n");
        }
        else
        {
            int size = q.size();
            for (int i = 0; i < size; i++)
            {
                if (!q.empty())
                {
                    processStruct tmp = q.front();
                    queue<string> tmpProcessValue = tmp.processValues;
                    int tmpSize = tmpProcessValue.size();
                    for (int j = 0; j < tmpSize; j++)
                    {
                        if (!tmpProcessValue.empty())
                        {
                            string val = tmpProcessValue.front();
                            tmpProcessValue.pop();
                            cout << "Oku bakalim1=" << val;
                        }
                    }
                    q.pop();
                    cout << endl;
                }
            }
        }

        if (TimerOrWaitFired)
        {
            //printf("The wait timed out.\n");
        }
        else
        {
            //printf("The wait event was signaled.\n");
        }
    }
    SetEvent(gDoneEvent1);
}

VOID CALLBACK TimerRoutine2(PVOID lpParam, BOOLEAN TimerOrWaitFired)
{
    if (lpParam == NULL)
    {
        printf("TimerRoutine lpParam is NULL\n");
    }
    else
    {
        queue<processStruct> q = *(queue<processStruct>*) lpParam;
        if (q.empty())
        {
            printf("OOOO222 LLLLL\n");
        }
        else
        {
            int size = q.size();
            for (int i = 0; i < size; i++)
            {
                if (!q.empty())
                {
                    processStruct tmp = q.front();
                    queue<string> tmpProcessValue = tmp.processValues;
                    int tmpSize = tmpProcessValue.size();
                    for (int j = 0; j < tmpSize; j++)
                    {
                        if (!tmpProcessValue.empty())
                        {
                            string val = tmpProcessValue.front();
                            tmpProcessValue.pop();
                            cout << "Oku bakalim2=" << val;
                        }
                    }
                    q.pop();
                    cout << endl;
                }
            }
        }


        if (TimerOrWaitFired)
        {
            //printf("The wait timed out.\n");
        }
        else
        {
            //printf("The wait event was signaled.\n");
        }
    }
    SetEvent(gDoneEvent2);
}
VOID CALLBACK TimerRoutine3(PVOID lpParam, BOOLEAN TimerOrWaitFired)
{
    if (lpParam == NULL)
    {
        printf("TimerRoutine lpParam is NULL\n");
    }
    else
    {
        queue<processStruct> q = *(queue<processStruct>*) lpParam;
        if (q.empty())
        {
            printf("OOOO3 LLLLL\n");
        }
        else
        {
            int size = q.size();
            for (int i = 0; i < size; i++)
            {
                if (!q.empty())
                {
                    processStruct tmp = q.front();
                    queue<string> tmpProcessValue = tmp.processValues;
                    int tmpSize = tmpProcessValue.size();
                    for (int j = 0; j < tmpSize; j++)
                    {
                        if (!tmpProcessValue.empty())
                        {
                            string val = tmpProcessValue.front();
                            tmpProcessValue.pop();
                            cout << "Oku bakalim3=" << val;
                        }
                    }
                    q.pop();
                    cout << endl;
                }
            }
        }

        if (TimerOrWaitFired)
        {
            //printf("The wait timed out.\n");
        }
        else
        {
            //printf("The wait event was signaled.\n");
        }
    }
    SetEvent(gDoneEvent3);
}

VOID CALLBACK TimerRoutine4(PVOID lpParam, BOOLEAN TimerOrWaitFired)
{
    if (lpParam == NULL)
    {
        printf("TimerRoutine lpParam is NULL\n");
    }
    else
    {
        queue<processStruct> q = *(queue<processStruct>*) lpParam;
        if (q.empty())
        {
            printf("OOOO44 LLLLL\n");
        }
        else
        {
            int size = q.size();
            for (int i = 0; i < size; i++)
            {
                if (!q.empty())
                {
                    processStruct tmp = q.front();
                    queue<string> tmpProcessValue = tmp.processValues;
                    int tmpSize = tmpProcessValue.size();
                    cout << "Oku bakalim4=";
                    for (int j = 0; j < tmpSize; j++)
                    {
                        if (!tmpProcessValue.empty())
                        {
                            string val = tmpProcessValue.front();
                            tmpProcessValue.pop();
                            cout <<  val;
                        }
                    }
                    q.pop();
                    cout << endl;
                }
            }
        }


        if (TimerOrWaitFired)
        {
            //printf("The wait timed out.\n");
        }
        else
        {
            //printf("The wait event was signaled.\n");
        }
    }
    SetEvent(gDoneEvent4);
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
    if (WaitForSingleObject(doneEvent, INFINITE) != WAIT_OBJECT_0)
    {
        printf("WaitForSingleObject failed (%d)\n", GetLastError());
    }
    else
    {

    }
}
//----------------------------------------------------------------------------------------


void setQueueValues(processStruct& PC,int val)
{
    string path = "p" + to_string(val) + ".txt";
    string tmp = "C:\\Users\\yustuntepe\\source\\repos\\mlfq\\Debug\\" + path;

    std::ifstream file(tmp);
    
    PC.priority = 4;
    PC.state = 10;

    if (file.is_open()) {
        std::string line;
        while (getline(file, line)) {    
            PC.processValues.push(line);
            printf("%s\n", line.c_str());
        }
        file.close();
    }
    else
    {
        cout << "Not Found!!!" << endl;
    }

}

void updateProcessStructPriority(processStruct& PC, int val)
{
    PC.priority = val;
}

void decideProcessPriorityQueue(vector<queue<processStruct>>& queueArray,processStruct& pcTmp,int newPriority)
{
    
    if (newPriority > 0)
    {
        queue<processStruct> tmp = queueArray[newPriority - 1];
        tmp.push(pcTmp);
        queueArray[newPriority - 1] = tmp;
    }
    else
    {
        printf("newPriority not valid");
    }
}


int main()
{
    HANDLE restTimer = NULL;
    HANDLE hTimer1 = NULL;
    HANDLE hTimer2 = NULL;
    HANDLE hTimer3 = NULL;
    HANDLE hTimer4 = NULL;
    
    HANDLE hTimerQueue = NULL;
    HANDLE hTimerQueueRest = NULL;
    

    int arg[3] = { 53,21,32 };
    int arg1 = 444;
    int arg2 = 555;

    //PROCESS 
    processStruct PC1;
    processStruct PC2;
    processStruct PC3;
    processStruct PC4;
    processStruct PC5;
    setQueueValues(PC1, 1);
    setQueueValues(PC2, 2);
    setQueueValues(PC3, 3);
    setQueueValues(PC4, 4);
    setQueueValues(PC5, 5);


    //QUEUE LIST
    queue<processStruct> myqueue1;
    queue<processStruct> myqueue2;
    queue<processStruct> myqueue3;
    queue<processStruct> myqueue4;

    vector<queue<processStruct>> queueArray = {};
    queueArray.push_back(myqueue1);
    queueArray.push_back(myqueue2);
    queueArray.push_back(myqueue3);
    queueArray.push_back(myqueue4);


    decideProcessPriorityQueue(queueArray, PC1, 4);
    decideProcessPriorityQueue(queueArray, PC2, 4);
    decideProcessPriorityQueue(queueArray, PC3, 4);
    decideProcessPriorityQueue(queueArray, PC4, 4);
    decideProcessPriorityQueue(queueArray, PC5, 4);


    // Use an event object to track the TimerRoutine execution
    
    createEvent(gDoneRestEvent);
    createEvent(gDoneEvent1);
    createEvent(gDoneEvent2);
    createEvent(gDoneEvent3);
    createEvent(gDoneEvent4);

    createEventQueue(hTimerQueueRest);
    createEventQueue(hTimerQueue);

    //--------------------------- REST VALUES---------------------------------
    if (!CreateTimerQueueTimer(&restTimer, hTimerQueueRest, (WAITORTIMERCALLBACK)TimerRoutineRest, &arg, 2000, 0, 0))
    {
        printf("CreateTimerQueueTimer failed (%d)\n", GetLastError());
        return 3;
    }

    //------------------------------ QUEUE ------------------------------------
    
    if (!CreateTimerQueueTimer(&hTimer1, hTimerQueue, (WAITORTIMERCALLBACK)TimerRoutine1, &queueArray[0], 100, 0, 0))
    {
        return 3;
    }
    if (!CreateTimerQueueTimer(&hTimer2, hTimerQueue, (WAITORTIMERCALLBACK)TimerRoutine2, &queueArray[1], 200, 0, 0))
    {
        return 3;
    }
    if (!CreateTimerQueueTimer(&hTimer3, hTimerQueue, (WAITORTIMERCALLBACK)TimerRoutine3, &queueArray[2], 300, 0, 0))
    {
        return 3;
    }
    //Priority FIRST
    if (!CreateTimerQueueTimer(&hTimer4, hTimerQueue, (WAITORTIMERCALLBACK)TimerRoutine4, &queueArray[3], 400, 0, 0))
    {
        return 3;
    }
    


    printf("Call timer routine in 10 seconds...\n");

    // Wait for the timer-queue thread to complete using an event 
    // object. The thread will signal the event at that time.

    waitSignalEvent(gDoneEvent1,"");
    CloseHandle(gDoneEvent1);

    waitSignalEvent(gDoneEvent2, "");
    CloseHandle(gDoneEvent2);

    waitSignalEvent(gDoneEvent3, "");
    CloseHandle(gDoneEvent3);

    waitSignalEvent(gDoneEvent4, "");
    CloseHandle(gDoneEvent4);

    waitSignalEvent(gDoneRestEvent, "");
    CloseHandle(gDoneRestEvent);


    // Delete all timers in the timer queue.
    if (!DeleteTimerQueue(hTimerQueue))
        printf("DeleteTimerQueue failed (%d)\n", GetLastError());

    if (!DeleteTimerQueue(hTimerQueueRest))
        printf("DeleteTimerQueue failed (%d)\n", GetLastError());

    return 0;
}