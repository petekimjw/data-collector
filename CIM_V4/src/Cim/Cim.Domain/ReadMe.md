# CIM V4

## 요구사항

### AddressMap
- Excel 파일을 쉽게 작성 및 변경 => 바로 적용버튼 클릭 (컨트롤러 추가, 어드레스맵 변경만)
- 데이터수집 버튼 => 엑셀 파일에 ByteOrder, DataType 변경에 따른 결과값 Write
- UI에서 어드레스맵 수정 및 적용
- 

### Excel
- 기존의 DocumentFormat.OpenXML 을 사용하여 코드 가독성 및 에러발견이 힘듬
- 기본 구현체에서 상속하여 쉽게 확장 가능하게 

- 파싱 잘못된값 표시 => 엑셀에 빨간색 배경 !!! (예: 중복주소, 주소오류(잘못문자), 공백, 특수문자 등)
- 엑셀안에 작성가이드용 이미지 표시해도 문제 없게 동작
- 셀 테두리가 없으면 오류, Row 인식 못하거나, Column경우 한칸씩 밀림
- 컬럼헤더가 고정 => 부가적인 설명이 있어도 동작하게 변경(예: ControllerName => Name(PLC장비이름))
- 컬럼의 위치가 고정되지 않고, 숨김 컬럼/로우해도 동작해야함
- 값이 비어 있으면 기본값 (이름, Address 주소 예외)
- 값이 여러가지 타입이여도 파싱하게 수정 (예: Scale = 100 or "0.00" or 2)
- 꼭 필요한 컬럼만 쓰고, 나머지는 MetaData

# 모니터링
- AddressMap과 데이터수집 모니터링을 한번에 표출


## ExcelAddressMapService
### ClosedXML
- https://github.com/closedxml/closedxml/wiki






