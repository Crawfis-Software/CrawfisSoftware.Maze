using CrawfisSoftware.Collections.Graph;
using System;
using System.Collections.Generic;

namespace CrawfisSoftware.Collections.Maze
{
    public class DungeonMakeBuilder<N, E> : MazeBuilderAbstract<N, E>
    {
        public struct Room
        {
            public int minX;
            public int minY;
            public int width;
            public int height;
            public Room(int minX, int minY, int width, int height)
            {
                this.minX = minX;
                this.minY = minY;
                this.width = width;
                this.height = height;
            }
        }
        public int NumberOfRooms { get; set; } = 2;
        public int MinRoomSize { get; set; } = 4;
        public int MaxRoomSize { get; set; } = 8;
        private List<Room> roomList = new List<Room>();
        private int minRoomDistance = 2;
        private int maxNumberOfTrys = 100000;

        public DungeonMakeBuilder(int width, int height, GetGridLabel<N> nodeAccessor, GetEdgeLabel<E> edgeAccessor)
            : base(width, height, nodeAccessor, edgeAccessor)
        {
            this.width = width;
            this.height = height;
        }

        public override void CreateMaze(bool preserveExistingCells)
        {
            if (!preserveExistingCells)
            {
                Clear();
            }
            MakeRooms();
            MakePassages();
        }

        private void MakePassages()
        {
            //throw new NotImplementedException();
        }

        private void MakeRooms()
        {
            CreateRandomRooms();
            foreach (Room room in roomList)
            {
                int lowerLeftIndex = room.minX + width * room.minY;
                int upperRightIndex = lowerLeftIndex + (room.height - 1) * width + (room.width - 1);
                Console.WriteLine("Creating Room {0}, {1}  to {2}, {3}", lowerLeftIndex % width, lowerLeftIndex / width, upperRightIndex % width, upperRightIndex / width);

                MakeRoom(lowerLeftIndex, upperRightIndex);
            }
        }

        private void MakeRoom(int lowerLeftIndex, int upperRightIndex)
        {
            int left = lowerLeftIndex % width;
            int right = upperRightIndex % width;
            int bottom = lowerLeftIndex / width;
            int top = upperRightIndex / width;
            // Set corners
            directions[left, bottom] |= Direction.N | Direction.E;
            directions[left, top] |= Direction.S | Direction.E;
            directions[right, bottom] |= Direction.N | Direction.W;
            directions[right, top] |= Direction.S | Direction.W;
            //int i, j;
            for (int i = left + 1; i < right; i++)
            {
                directions[i, bottom] |= Direction.W | Direction.N | Direction.E;
                directions[i, top] |= Direction.W | Direction.E | Direction.S;
            }
            for (int j = bottom + 1; j < top; j++)
            {
                directions[left, j] |= Direction.N | Direction.E | Direction.S;
                directions[right, j] |= Direction.W | Direction.N | Direction.S;
            }
            for (int i = left + 1; i < right; i++)
            {
                for (int j = bottom + 1; j < top; j++)
                {
                    directions[i, j] |= Direction.W | Direction.N | Direction.E | Direction.S;
                }
            }
        }

        private void CreateRandomRooms()
        {
            int deltaWidth = MaxRoomSize - MinRoomSize + 1;
            int minimumXCoord = width - MaxRoomSize;
            int minumumYCoord = height - MaxRoomSize;
            // Random create rooms
            int roomTrys = 0;
            while (roomList.Count < this.NumberOfRooms && roomTrys < maxNumberOfTrys)
            {
                int roomWidth = MinRoomSize + RandomGenerator.Next(deltaWidth);
                int roomHeight = MinRoomSize + RandomGenerator.Next(deltaWidth);
                int minX = RandomGenerator.Next(minimumXCoord);
                int minY = RandomGenerator.Next(minumumYCoord);
                Room room = new Room(minX, minY, roomWidth, roomHeight);
                roomTrys++;
                // Ensure they are minRoomDistance apart.
                int minDistance = width + height;
                foreach (Room placedRoom in roomList)
                {
                    int distance = RoomDistance(placedRoom, room);
                    if (distance < minRoomDistance)
                    {
                        // Move Room
                        minDistance = distance;
                    }
                }
                //if (minDistance > minRoomDistance)
                {
                    roomList.Add(room);
                }
            }
        }

        private static int RoomDistance(Room room1, Room room2)
        {
            int xDistance = 0;
            int yDistance = 0;
            int x1 = room1.minX;
            int x2 = x1 + room1.width;
            int y1 = room2.minY;
            int y2 = y1 + room1.height;
            int u1 = room2.minX;
            int u2 = u1 + room2.width;
            int v1 = room2.minY;
            int v2 = v1 + room2.height;
            if (x2 < u1)
            {
                xDistance = u1 - x2;
            }
            else if (u2 < x1)
            {
                xDistance = x1 - u2;
            }
            if (y2 < v1)
            {
                yDistance = v1 - y2;
            }
            else if (v2 < y1)
            {
                yDistance = y1 - v2;
            }
            return xDistance + yDistance;
        }
    }
}
