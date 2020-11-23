# .NET Core vs Golang performance

This repository holds code to test http api performance between .NET Core and Golang HTTP.
Each service has `/test` endpoint which calls another api using http client and returns that api response as JSON.

## Start containers

`docker-compose up --build`

docker-compose should start 3 containers
1) Golang api with GET `http://localhost:5002/data` endpoint
2) Golang api with GET `http://localhost:5001/test` endpoint which calls 1 endpoint
3) .NET Core api with GET `http://localhost:5000/test` endpoint which calls 1 endpoint

## Run load tests

```
brew install wrk
cd wrk
// .net core
URL=http://localhost:5000 make run

// golang
URL=http://localhost:5001 make run
```

## Check for file descriptors leaks

Connect to docker container while wrk is running
`docker exec -it <CONTAINER_ID> /bin/bash`

Count TIME_WAIT state
`ss -t4 state time-wait | wc -l`

## Results

### .net core api (http://localhost:5000)

```
Running 2m test @ http://localhost:5000
  8 threads and 256 connections
  Thread Stats   Avg      Stdev     Max   +/- Stdev
    Latency    52.35ms   72.08ms   1.37s    94.27%
    Req/Sec   769.33    150.89     1.41k    71.93%
  Latency Distribution
     50%   37.67ms
     75%   55.92ms
     90%   93.95ms
     99%  268.29ms
  606631 requests in 1.67m, 131.19MB read
  Socket errors: connect 0, read 113, write 0, timeout 0
Requests/sec:   6061.97
Transfer/sec:      1.31MB
```

Resources used
```
CPU: 100%
MEMORY: 82MB
TIME_WAIT file descriptors: ~3
```

### golang api (http://localhost:5001)

```
Running 2m test @ http://localhost:5001
  8 threads and 256 connections
  Thread Stats   Avg      Stdev     Max   +/- Stdev
    Latency    72.61ms   42.65ms   1.16s    71.59%
    Req/Sec   450.50     81.78     0.87k    70.70%
  Latency Distribution
     50%   70.84ms
     75%   97.67ms
     90%  122.23ms
     99%  178.06ms
  358931 requests in 1.67m, 65.30MB read
  Socket errors: connect 0, read 113, write 0, timeout 0
Requests/sec:   3587.32
Transfer/sec:    668.27KB
```

Resources used
```
CPU: 100%
MEMORY: 25.57MB
TIME_WAIT file descriptors: ~10
```

## My machine spec

* MacBook Pro (16-inch, 2019)
* Processor 2,6 GHz Intel Core i7
* Memory 16 GB 2667 MHz LPDDR3
* Docker version 19.03.0-ce (4 CPUs, 2 GiB memory)
* Golang 1.15.2
* Dotnet 5.0.0
