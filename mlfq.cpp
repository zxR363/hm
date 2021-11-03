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
    string name;
    queue<string> processValues;
    int priority;
    int state; //10 - init ,11- "0" ,12- "1",13 - "-" karsilik
};

void decideProcessPriorityQueue(vector<queue<processStruct>>& queueArray, processStruct& pcTmp, int newPriority)
{

    if (newPriority > 0)
    {
        queue<processStruct> tmp = queueArray[newPriority - 1];
        pcTmp.priority = newPriority;
        tmp.push(pcTmp);
        queueArray[newPriority - 1] = tmp;
        cout << endl;
    } 
    else
    {
        printf("newPriority not valid");
    }
}

VOID CALLBACK TimerRoutineRest(PVOID lpParam, BOOLEAN TimerOrWaitFired)
{
    if (lpParam == NULL)
    {
        printf("TimerRoutine lpParam is NULL\n");
    }
    else
    {
        /*
        vector<queue<processStruct>>* qArray = (vector<queue<processStruct>>*) lpParam;        
        vector<queue<processStruct>>& tmp = *qArray;
        for (int i = 0; i < tmp.size(); i++)
        {
            queue<processStruct>* q = &tmp[i];
            queue<processStruct>& tmpQ = *q;
            for (int j = 0; j < tmpQ.size(); j++)
            {
                processStruct* tmp = &tmpQ->front();
                q->pop();
            }
        }
        */
        vector<queue<processStruct>>* qArray = (vector<queue<processStruct>>*) lpParam;
        //queue<processStruct> q = *(queue<processStruct>*) lpParam;
        vector<queue<processStruct>>& tmp = *qArray;
        for (int j = 0; j < tmp.size(); j++)
        {
            queue<processStruct>* q = &tmp[j];

            if (q->empty())
            {

            }
            else
            {
                int size = q->size();
                for (int i = 0; i < size; i++)
                {
                    if (!q->empty())
                    {
                        processStruct* tmp = &q->front();
                        decideProcessPriorityQueue(*qArray, *tmp, 4);
                        q->pop();
                    }
                }
            }
        }
        cout << "Refresh Priority" << endl;


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
        vector<queue<processStruct>>* qArray = (vector<queue<processStruct>>*) lpParam;
        //queue<processStruct> q = *(queue<processStruct>*) lpParam;
        vector<queue<processStruct>>& tmp = *qArray;
        queue<processStruct>* q = &tmp[0];

        if (q->empty())
        {

        }
        else
        {
            int size = q->size();
            for (int i = 0; i < size; i++)
            {
                if (!q->empty())
                {
                    processStruct* tmp = &q->front();
                    queue<string>* tmpProcessValue = &(*tmp).processValues;
                    int tmpSize = tmpProcessValue->size();

                    if (!tmpProcessValue->empty())
                    {
                        string val = tmpProcessValue->front();
                        int processValSize = tmpProcessValue->size();
                        tmpProcessValue->pop();
                        if (processValSize == 1)
                        {
                            if (val == "-") // Process bitiyor
                            {
                                cout << "E" << "," << tmp->name << ",QX" << endl;
                                q->pop();
                            }
                        }
                        else
                        {
                            if (val == "1")
                            {
                                cout << val << "," << tmp->name << ",Q1" << endl;
                                decideProcessPriorityQueue(*qArray, *tmp, (*tmp).priority); //En state
                                q->pop();
                            }
                            else if (val == "0")
                            {
                                cout << val << "," << tmp->name << ",Q1" << endl;
                                decideProcessPriorityQueue(*qArray, *tmp, (*tmp).priority);
                                //q->push(*tmp);
                                q->pop();
                            }


                            cout << "";
                        }

                    }
                    else
                    {
                        q->pop(); //Process Bittiyse
                    }
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

        vector<queue<processStruct>>* qArray = (vector<queue<processStruct>>*) lpParam;
        //queue<processStruct> q = *(queue<processStruct>*) lpParam;
        vector<queue<processStruct>>& tmp = *qArray;
        queue<processStruct>* q = &tmp[1];

        if (q->empty())
        {

        }
        else
        {
            int size = q->size();
            for (int i = 0; i < size; i++)
            {
                if (!q->empty())
                {
                    processStruct* tmp = &q->front();
                    queue<string>* tmpProcessValue = &(*tmp).processValues;
                    int tmpSize = tmpProcessValue->size();

                    if (!tmpProcessValue->empty())
                    {
                        string val = tmpProcessValue->front();
                        int processValSize = tmpProcessValue->size();
                        tmpProcessValue->pop();
                        if (processValSize == 1)
                        {
                            if (val == "-") // Process bitiyor
                            {
                                cout << "E" << "," << tmp->name << ",QX" << endl;
                                q->pop();
                            }
                        }
                        else
                        {
                            if (val == "1")
                            {
                                cout << val << "," << tmp->name << ",Q2" << endl;
                                decideProcessPriorityQueue(*qArray, *tmp, (*tmp).priority - 1);
                                q->pop();
                            }
                            else if (val == "0")
                            {
                                cout << val << "," << tmp->name << ",Q2" << endl;
                                decideProcessPriorityQueue(*qArray, *tmp, (*tmp).priority);
                                //q->push(*tmp);
                                q->pop();
                            }


                            cout << "";
                        }

                    }
                    else
                    {
                        q->pop(); //Process Bittiyse
                    }
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

        vector<queue<processStruct>>* qArray = (vector<queue<processStruct>>*) lpParam;
        //queue<processStruct> q = *(queue<processStruct>*) lpParam;
        vector<queue<processStruct>>& tmp = *qArray;
        queue<processStruct>* q = &tmp[2];

        if (q->empty())
        {

        }
        else
        {
            int size = q->size();
            for (int i = 0; i < size; i++)
            {
                if (!q->empty())
                {
                    processStruct* tmp = &q->front();
                    queue<string>* tmpProcessValue = &(*tmp).processValues;
                    int tmpSize = tmpProcessValue->size();

                    if (!tmpProcessValue->empty())
                    {
                        string val = tmpProcessValue->front();
                        int processValSize = tmpProcessValue->size();
                        tmpProcessValue->pop();
                        if (processValSize == 1)
                        {
                            if (val == "-") // Process bitiyor
                            {
                                cout << "E" << "," << tmp->name << ",QX" << endl;
                                q->pop();
                            }
                        }
                        else
                        {
                            if (val == "1")
                            {
                                cout << val << "," << tmp->name << ",Q3" << endl;
                                decideProcessPriorityQueue(*qArray, *tmp, (*tmp).priority - 1);
                                q->pop();
                            }
                            else if (val == "0")
                            {
                                cout << val << "," << tmp->name << ",Q3" << endl;
                                decideProcessPriorityQueue(*qArray, *tmp, (*tmp).priority);
                                //q->push(*tmp);
                                q->pop();
                            }


                            cout << "";
                        }

                    }
                    else
                    {
                        q->pop(); //Process Bittiyse
                    }

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
        vector<queue<processStruct>>* qArray = (vector<queue<processStruct>>*) lpParam;
        //queue<processStruct> q = *(queue<processStruct>*) lpParam;
        vector<queue<processStruct>>&tmp = *qArray;
        queue<processStruct>* q = &tmp[3];

        if (q->empty())
        {
            
        }
        else
        {
            int size = q->size();
            for (int i = 0; i < size; i++)
            {
                if (!q->empty())
                {
                    processStruct* tmp = &q->front();
                    queue<string>* tmpProcessValue = &(*tmp).processValues;
                    int tmpSize = tmpProcessValue->size();
                        
                    if (!tmpProcessValue->empty())
                    {
                        string val = tmpProcessValue->front();
                        int processValSize = tmpProcessValue->size();
                        tmpProcessValue->pop();
                        if (processValSize == 1)
                        {
                            if (val == "-") // Process bitiyor
                            {
                                cout << "E" << "," << tmp->name << ",QX" << endl;
                                q->pop();
                            }
                        }
                        else
                        {
                            if (val == "1")
                            {
                                cout << val << "," << tmp->name << ",Q4" << endl;
                                decideProcessPriorityQueue(*qArray, *tmp, (*tmp).priority - 1);
                                q->pop();
                            }
                            else if(val == "0")
                            {
                                cout << val << "," << tmp->name << ",Q4" << endl;
                                decideProcessPriorityQueue(*qArray, *tmp, (*tmp).priority);
                                //q->push(*tmp);
                                q->pop();
                            }
                            
                            
                            cout << "";
                        }
                        
                    }
                    else
                    {
                        q->pop(); //Process Bittiyse
                    }

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
    PC1.name = "PC1";
    PC2.name = "PC2";
    PC3.name = "PC3";
    PC4.name = "PC4";
    PC5.name = "PC5";
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
    
    


    createEventQueue(hTimerQueueRest);
    createEventQueue(hTimerQueue);

    //------------------------------ QUEUE ------------------------------------
    while( !(queueArray[0].empty() && queueArray[1].empty() && queueArray[2].empty() && queueArray[3].empty()))
    {
        createEvent(gDoneRestEvent);
        createEvent(gDoneEvent1);
        createEvent(gDoneEvent2);
        createEvent(gDoneEvent3);
        createEvent(gDoneEvent4);

        //--------------------------- REST VALUES---------------------------------
        if (!CreateTimerQueueTimer(&restTimer, hTimerQueueRest, (WAITORTIMERCALLBACK)TimerRoutineRest, &queueArray, 700, 0, 0))
        {
            return 3;
        }

        if (!CreateTimerQueueTimer(&hTimer1, hTimerQueue, (WAITORTIMERCALLBACK)TimerRoutine1, &queueArray, 0, 0, 0))
        {
            return 3;
        }
        if (!CreateTimerQueueTimer(&hTimer2, hTimerQueue, (WAITORTIMERCALLBACK)TimerRoutine2, &queueArray, 100, 0, 0))
        {
            return 3;
        }
        if (!CreateTimerQueueTimer(&hTimer3, hTimerQueue, (WAITORTIMERCALLBACK)TimerRoutine3, &queueArray, 200, 0, 0))
        {
            return 3;
        }
        //Priority FIRST
        if (!CreateTimerQueueTimer(&hTimer4, hTimerQueue, (WAITORTIMERCALLBACK)TimerRoutine4, &queueArray, 300, 0, 0))
        {
            return 3;
        }

        waitSignalEvent(gDoneEvent1, "");
        CloseHandle(gDoneEvent1);

        waitSignalEvent(gDoneEvent2, "");
        CloseHandle(gDoneEvent2);

        waitSignalEvent(gDoneEvent3, "");
        CloseHandle(gDoneEvent3);

        waitSignalEvent(gDoneEvent4, "");
        CloseHandle(gDoneEvent4);

        waitSignalEvent(gDoneRestEvent, "");
        CloseHandle(gDoneRestEvent);

    }

    // Delete all timers in the timer queue.
    if (!DeleteTimerQueue(hTimerQueue))
        printf("DeleteTimerQueue failed (%d)\n", GetLastError());

    if (!DeleteTimerQueue(hTimerQueueRest))
        printf("DeleteTimerQueue failed (%d)\n", GetLastError());

    return 0;
}