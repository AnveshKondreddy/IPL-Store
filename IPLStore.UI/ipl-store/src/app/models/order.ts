export interface OrderItem {
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface Order {
  id: number;
  userId: string;
  orderedAt: string;
  totalAmount: number;
  items: OrderItem[];
}
