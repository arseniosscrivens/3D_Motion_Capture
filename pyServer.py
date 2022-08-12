from concurrent import futures
import grpc
import cvTransfer_pb2
import cvTransfer_pb2_grpc

#called whenever client requests data from server.
class PointSender(cvTransfer_pb2_grpc.PointSenderServicer):

    setOfPoints = cvTransfer_pb2.AllPoints()
    dummyID = cvTransfer_pb2.dummyPoint(id=1)

    #adds all 21 points and defaults to 1.0
    def __init__(self):
        for i in range(21):
            self.setOfPoints.vec.add()
            self.setOfPoints.vec[i].x = 1.0
            self.setOfPoints.vec[i].y = 1.0
            self.setOfPoints.vec[i].z = 1.0

    def GetPoints(self, request, context):
        print("Hello from GetPoints.")
        return self.setOfPoints

    def SetPoints(self, request, context):
        for i in range(21):
            self.setOfPoints.vec[i].x = request.vec[i].x
            self.setOfPoints.vec[i].y = request.vec[i].y
            self.setOfPoints.vec[i].z = request.vec[i].z
        print("Hello from SetPoints.")
        return self.dummyID


server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
cvTransfer_pb2_grpc.add_PointSenderServicer_to_server(PointSender(), server)
server.add_insecure_port('[::]:50051')
server.start()
server.wait_for_termination()
