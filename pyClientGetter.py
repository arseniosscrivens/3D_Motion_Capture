import grpc
import cvTransfer_pb2
import cvTransfer_pb2_grpc

with grpc.insecure_channel('localhost:50051') as channel:
    stub = cvTransfer_pb2_grpc.PointSenderStub(channel)
    response = stub.GetPoints(cvTransfer_pb2.dummyPoint(id=1))
print(response.vec[0].x)
print(response.vec[1].x)
